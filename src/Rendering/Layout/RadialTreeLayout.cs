using System.Numerics;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Layout;

/// <summary>
/// Gource-style radial tree layout.
///
/// Buduje wirtualne drzewo katalogów bezpośrednio z ścieżek węzłów File,
/// ponieważ węzły Branch to gałęzie gita (nie katalogi filesystemu).
/// Węzły Branch (gałęzie gita) są rozmieszczone osobno wokół centrum.
/// Linie (krawędzie) są rysowane tylko między katalogami, nie do plików.
/// 
/// Wynik layoutu zawiera również mapę połączeń między katalogami do rysowania linii.
/// </summary>
internal sealed class RadialTreeLayout : ILayoutEngine
{
    // Stałe layoutu
    private const float DirLevelRadius = 400f;   // odległość między poziomami katalogów
    private const float FileOrbit      =  80f;   // promień orbit pliku wokół katalogu
    private const float BranchOrbit    = 500f;   // gałęzie gita — daleki pierścień
    private const float MinSectorAngle = 0.3f;   // minimalny kąt sektora (w radianach)

    // Publiczna właściwość do odczytania połączeń między katalogami
    public IReadOnlyDictionary<string, string> DirectoryConnections => _directoryParentMap;
    
    // Śledzenie relacji rodzic-dziecko dla katalogów
    private readonly Dictionary<string, string> _directoryParentMap = new();

    public void Step(IReadOnlyDictionary<string, SceneNode> nodes, float deltaSeconds)
    {
        if (nodes.Count == 0) return;

        var allNodes = nodes.Values.ToList();
        var fileNodes   = allNodes.Where(n => n.Kind == NodeKind.File).ToList();
        var branchNodes = allNodes.Where(n => n.Kind == NodeKind.Branch).ToList();
        var rootNodes   = allNodes.Where(n => n.Kind == NodeKind.Root).ToList();

        // ---- 1. Buduj wirtualne drzewo z ścieżek plików ----
        _directoryParentMap.Clear();
        var dirPositions = BuildVirtualDirectoryTree(fileNodes);

        // ---- 2. Przypisz pliki do ich katalogów ----
        var filesByDir = fileNodes.GroupBy(n => GetParentPath(n.Id)).ToList();
        foreach (var group in filesByDir)
        {
            if (!dirPositions.TryGetValue(group.Key, out var dirPos))
                continue;
                
            var files  = group.OrderBy(n => n.Id).ToList();
            
            // Rozłóż pliki równomiernie wokół katalogu
            float step = files.Count > 0 ? MathF.Tau / files.Count : MathF.Tau;
            float angle = 0f;
            foreach (var file in files)
            {
                file.HomePosition = dirPos + new Vec2(
                    MathF.Cos(angle) * FileOrbit,
                    MathF.Sin(angle) * FileOrbit);
                file.Pinned = true;
                angle += step;
            }
        }

        // ---- 3. Gałęzie gita na zewnętrznym pierścieniu ----
        if (branchNodes.Count > 0)
        {
            float step  = MathF.Tau / branchNodes.Count;
            float angle = 0f;
            foreach (var branch in branchNodes.OrderBy(n => n.Id))
            {
                branch.HomePosition = new Vec2(
                    MathF.Cos(angle) * BranchOrbit,
                    MathF.Sin(angle) * BranchOrbit);
                branch.Pinned = true;
                angle += step;
            }
        }

        // ---- 4. Explicit Root nodes (jeśli są) → centrum ----
        foreach (var root in rootNodes)
        {
            root.HomePosition = Vec2.Zero;
            root.Pinned = true;
        }

        // ---- 5. Zastosuj pozycje ----
        foreach (var node in allNodes)
        {
            node.Position = node.HomePosition;
            node.Velocity = Vec2.Zero;
        }
    }

    // ------------------------------------------------------------------
    // Budowanie wirtualnego drzewa katalogów
    // Zwraca słownik: ścieżka_katalogu → pozycja w przestrzeni 2D
    // ------------------------------------------------------------------

    private Dictionary<string, Vec2> BuildVirtualDirectoryTree(List<SceneNode> fileNodes)
    {
        // Zbierz wszystkie unikalne katalogi ze ścieżek plików
        var allDirs = new HashSet<string>();
        foreach (var file in fileNodes)
        {
            var path = GetParentPath(file.Id);
            while (!string.IsNullOrEmpty(path))
            {
                allDirs.Add(path);
                path = GetParentPath(path);
            }
            allDirs.Add(""); // root katalogu
        }

        // Zbuduj mapę parent→children dla katalogów
        var children = new Dictionary<string, List<string>>();
        children[""] = new List<string>();
        foreach (var dir in allDirs)
        {
            if (!children.ContainsKey(dir))
                children[dir] = new List<string>();
            
            var parent = GetParentPath(dir);
            if (!children.ContainsKey(parent))
                children[parent] = new List<string>();
            
            if (dir != parent && !string.IsNullOrEmpty(dir))
            {
                children[parent].Add(dir);
                // Zapisz relację rodzic-dziecko dla katalogów
                // To będzie używane do rysowania linii
                _directoryParentMap[dir] = parent;
            }
        }

        // Usuń duplikaty dzieci
        foreach (var key in children.Keys.ToList())
            children[key] = children[key].Distinct().ToList();

        // Oblicz wagę (liczbę plików) dla każdego katalogu
        var weights = CalculateSubtreeWeights("", children, fileNodes);

        // Rekurencyjnie przypisz pozycje zaczynając od roota (pusta ścieżka)
        var positions = new Dictionary<string, Vec2>();
        positions[""] = Vec2.Zero;

        LayoutDirRecursive("", Vec2.Zero, 0f, MathF.Tau, depth: 1, children, weights, positions);

        return positions;
    }

    private Dictionary<string, int> CalculateSubtreeWeights(
        string root, 
        Dictionary<string, List<string>> children, 
        List<SceneNode> fileNodes)
    {
        var weights = new Dictionary<string, int>();
        
        // Najpierw przypisz wagi dla plików (liści)
        var filesByDir = fileNodes.GroupBy(n => GetParentPath(n.Id));
        foreach (var group in filesByDir)
        {
            weights[group.Key] = group.Count();
        }

        // Rekurencyjnie sumuj wagi od liści do korzenia
        SumWeightsRecursive(root, children, weights);
        
        return weights;
    }

    private int SumWeightsRecursive(
        string dirId, 
        Dictionary<string, List<string>> children, 
        Dictionary<string, int> weights)
    {
        if (!children.ContainsKey(dirId)) 
            return weights.GetValueOrDefault(dirId, 1); // Domyślna waga 1 dla katalogów bez plików

        int totalWeight = weights.GetValueOrDefault(dirId, 0);
        
        foreach (var child in children[dirId])
        {
            totalWeight += SumWeightsRecursive(child, children, weights);
        }
        
        weights[dirId] = totalWeight > 0 ? totalWeight : 1; // Minimalna waga to 1
        return weights[dirId];
    }

    private void LayoutDirRecursive(
        string dirId, 
        Vec2 centre,
        float startAngle, 
        float endAngle,
        int depth,
        Dictionary<string, List<string>> children,
        Dictionary<string, int> weights,
        Dictionary<string, Vec2> positions)
    {
        if (!children.TryGetValue(dirId, out var kids) || kids.Count == 0)
            return;

        kids = kids.OrderBy(k => k).ToList();
        
        // Oblicz całkowitą wagę dla dzieci
        int totalWeight = kids.Sum(k => weights.GetValueOrDefault(k, 1));
        
        float currentAngle = startAngle;
        float sectorSize = endAngle - startAngle;

        foreach (var kid in kids)
        {
            // Rozmiar sektora proporcjonalny do wagi poddrzewa
            float kidWeight = weights.GetValueOrDefault(kid, 1);
            float kidSector = sectorSize * (kidWeight / (float)totalWeight);
            
            // Minimalny rozmiar sektora
            kidSector = MathF.Max(kidSector, MinSectorAngle);
            
            // Środek sektora dla tego dziecka
            float midAngle = currentAngle + kidSector * 0.5f;
            
            // Oblicz pozycję na okręgu
            float radius = depth * DirLevelRadius;
            var pos = centre + new Vec2(
                MathF.Cos(midAngle) * radius, 
                MathF.Sin(midAngle) * radius);

            positions[kid] = pos;
            
            // Rekurencyjnie rozłóż dzieci tego katalogu
            LayoutDirRecursive(
                kid, 
                pos, 
                currentAngle, 
                currentAngle + kidSector, 
                depth + 1, 
                children, 
                weights, 
                positions);
            
            currentAngle += kidSector;
        }
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private static string GetParentPath(string id)
    {
        int idx = id.LastIndexOf('/');
        return idx < 0 ? "" : id[..idx];
    }
}