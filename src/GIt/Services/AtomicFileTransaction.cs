using ChangeTrace.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ChangeTrace.GIt.Services;

/// <summary>
/// Atomic file transaction for staged writes and deletes.
/// </summary>
internal sealed class AtomicFileTransaction : IAsyncDisposable
{
    private readonly IFileManager _fileManager;
    private readonly Dictionary<string, FileOperation> _operations = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _operationOrder = [];
    private readonly ILogger? _logger;
    private bool _committed;
    private bool _disposed;

    /// <summary>
    /// Creates a transaction with optional debug logging.
    /// </summary>
    internal AtomicFileTransaction(IFileManager fileManager, ILogger? logger = null)
    {
        _fileManager = fileManager;
        _logger = logger;
    }

    /// <summary>
    /// Stages a file write and returns a temp stream.
    /// </summary>
    internal Task<Stream> OpenWriteAsync(string finalPath, CancellationToken cancellationToken = default)
    {
        var tempPath = CreateSiblingPath(finalPath, ".tmp");
        RegisterOperation(FileOperation.Write(finalPath, tempPath));
        return _fileManager.OpenWriteAsync(tempPath, cancellationToken);
    }

    /// <summary>
    /// Stages byte content for a file.
    /// </summary>
    internal async Task WriteBytesAsync(string finalPath, byte[] data, CancellationToken cancellationToken = default)
    {
        var tempPath = CreateSiblingPath(finalPath, ".tmp");
        RegisterOperation(FileOperation.Write(finalPath, tempPath));
        await _fileManager.SaveAsync(tempPath, data, cancellationToken);
    }

    /// <summary>
    /// Stages deletion of an existing file.
    /// </summary>
    internal void Delete(string finalPath)
        => RegisterOperation(FileOperation.Delete(finalPath));

    /// <summary>
    /// Commits all staged operations atomically.
    /// </summary>
    internal Task CommitAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(AtomicFileTransaction));
        
        try
        {
            CommitCore();
            _committed = true;
            return Task.CompletedTask;
        }
        catch
        {
            RollbackCore();
            throw;
        }
    }

    /// <summary>
    /// Rolls back staged work when the transaction is not committed.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        if (_disposed)
            return ValueTask.CompletedTask;

        if (!_committed)
            RollbackCore();

        _disposed = true;
        return ValueTask.CompletedTask;
    }

    private void CommitCore()
    {
        if (_operationOrder.Count == 0)
            return;

        foreach (var path in _operationOrder)
        {
            var operation = _operations[path];
            if (operation.Kind != FileOperationKind.Delete)
                continue;

            if (!File.Exists(operation.FinalPath))
                continue;

            operation.BackupPath = CreateSiblingPath(operation.FinalPath, ".bak");
            File.Move(operation.FinalPath, operation.BackupPath, true);
        }

        foreach (var path in _operationOrder)
        {
            var operation = _operations[path];
            if (operation.Kind != FileOperationKind.Write)
                continue;

            if (File.Exists(operation.FinalPath))
            {
                operation.BackupPath = CreateSiblingPath(operation.FinalPath, ".bak");
                File.Move(operation.FinalPath, operation.BackupPath, true);
            }

            File.Move(operation.TempPath!, operation.FinalPath, true);
        }

        foreach (var path in _operationOrder)
        {
            var operation = _operations[path];
            if (string.IsNullOrWhiteSpace(operation.BackupPath) || !File.Exists(operation.BackupPath))
                continue;

            File.Delete(operation.BackupPath);
            operation.BackupPath = null;
        }
    }

    private void RollbackCore()
    {
        if (_operationOrder.Count == 0)
            return;

        for (var index = _operationOrder.Count - 1; index >= 0; index--)
        {
            var operation = _operations[_operationOrder[index]];

            try
            {
                if (operation.Kind == FileOperationKind.Write)
                    RollbackWrite(operation);
                else
                    RollbackDelete(operation);
            }
            catch (Exception ex)
            {
                if (_logger is not null)
                {
                    _logger.LogDebug(
                        ex,
                        "Best effort rollback failed for {Path}; continuing cleanup.",
                        operation.FinalPath);
                }
            }
        }

        CleanupStagedFiles();
    }

    private static void RollbackWrite(FileOperation operation)
    {
        if (!string.IsNullOrWhiteSpace(operation.BackupPath) && File.Exists(operation.BackupPath))
        {
            if (File.Exists(operation.FinalPath))
                File.Delete(operation.FinalPath);

            File.Move(operation.BackupPath, operation.FinalPath, true);
            operation.BackupPath = null;
        }

        if (!string.IsNullOrWhiteSpace(operation.TempPath) && File.Exists(operation.TempPath))
            File.Delete(operation.TempPath);
    }

    private static void RollbackDelete(FileOperation operation)
    {
        if (string.IsNullOrWhiteSpace(operation.BackupPath) || !File.Exists(operation.BackupPath))
            return;

        if (File.Exists(operation.FinalPath))
            File.Delete(operation.FinalPath);

        File.Move(operation.BackupPath, operation.FinalPath, true);
        operation.BackupPath = null;
    }

    private void CleanupStagedFiles()
    {
        if (_operations.Count == 0)
            return;

        foreach (var operation in _operations.Values)
        {
            if (!string.IsNullOrWhiteSpace(operation.TempPath) && File.Exists(operation.TempPath))
                File.Delete(operation.TempPath);

            if (!string.IsNullOrWhiteSpace(operation.BackupPath) && File.Exists(operation.BackupPath))
                File.Delete(operation.BackupPath);
        }
    }

    private static string CreateSiblingPath(string finalPath, string suffix)
    {
        var directory = Path.GetDirectoryName(finalPath) ?? string.Empty;
        var fileName = Path.GetFileName(finalPath);
        var stagedName = $"{fileName}{suffix}.{Guid.NewGuid():N}";
        return string.IsNullOrEmpty(directory)
            ? stagedName
            : Path.Combine(directory, stagedName);
    }

    private void RegisterOperation(FileOperation operation)
    {
        if (_operations.TryGetValue(operation.FinalPath, out var existing))
        {
            if (!string.IsNullOrWhiteSpace(existing.TempPath) &&
                !string.Equals(existing.TempPath, operation.TempPath, StringComparison.OrdinalIgnoreCase) &&
                File.Exists(existing.TempPath))
            {
                File.Delete(existing.TempPath);
            }

            _operations[operation.FinalPath] = operation;
            return;
        }

        _operations.Add(operation.FinalPath, operation);
        _operationOrder.Add(operation.FinalPath);
    }

    private sealed class FileOperation(FileOperationKind kind, string finalPath, string? tempPath)
    {
        public FileOperationKind Kind { get; } = kind;
        public string FinalPath { get; } = finalPath;
        public string? TempPath { get; } = tempPath;
        public string? BackupPath { get; set; }

        public static FileOperation Write(string finalPath, string tempPath)
            => new(FileOperationKind.Write, finalPath, tempPath);

        public static FileOperation Delete(string finalPath)
            => new(FileOperationKind.Delete, finalPath, null);
    }

    private enum FileOperationKind
    {
        Write,
        Delete
    }
}
