using ChangeTrace.CredentialTrace.Interfaces;
using ChangeTrace.CredentialTrace.Profiles;

namespace ChangeTrace.CredentialTrace.Services;

internal interface IProfileStore<T>
    where T : class, IProfile
{
    Task SaveAsync(T profile, CancellationToken ct = default);
    Task DeleteAsync(Ulid id, CancellationToken ct = default);
    Task<T?> GetAsync(Ulid id, CancellationToken ct = default);
    Task<T?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
}