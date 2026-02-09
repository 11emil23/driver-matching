using System.Collections.Generic;

namespace DriverMatching.Core;

/// <summary>
/// Stores drivers on an N*M grid with invariant: at most one driver per cell.
/// </summary>
public sealed class DriverStore
{
    private readonly int _n;
    private readonly int _m;
    private readonly int[,] _grid;
    private readonly Dictionary<int, (int X, int Y)> _byId = new();

    public DriverStore(int n, int m)
    {
        if (n <= 0 || m <= 0) throw new ArgumentOutOfRangeException();
        _n = n; _m = m;

        _grid = new int[n, m];
        for (int x = 0; x < n; x++)
            for (int y = 0; y < m; y++)
                _grid[x, y] = -1;
    }

    public int N => _n;
    public int M => _m;

    public IReadOnlyDictionary<int, (int X, int Y)> DriversById => _byId;
    public int[,] Grid => _grid;

    public void Upsert(int id, int x, int y)
    {
        ValidateXY(x, y);

        if (_byId.TryGetValue(id, out var old))
            _grid[old.X, old.Y] = -1;

        int existing = _grid[x, y];
        if (existing != -1 && existing != id)
            throw new InvalidOperationException($"Cell ({x},{y}) already occupied by driver {existing}.");

        _grid[x, y] = id;
        _byId[id] = (x, y);
    }

    public bool Remove(int id)
    {
        if (!_byId.TryGetValue(id, out var pos))
            return false;

        _grid[pos.X, pos.Y] = -1;
        _byId.Remove(id);
        return true;
    }

    private void ValidateXY(int x, int y)
    {
        if ((uint)x >= (uint)_n || (uint)y >= (uint)_m)
            throw new ArgumentOutOfRangeException($"Out of bounds: ({x},{y}) for grid {_n}x{_m}");
    }
}
