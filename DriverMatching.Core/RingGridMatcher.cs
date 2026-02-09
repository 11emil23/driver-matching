namespace DriverMatching.Core;

/// <summary>
/// Algorithm 2: Expanding square rings over grid cells (fast on dense maps).
/// Uses a safe check against DriversById to avoid any inconsistent ids.
/// </summary>
public sealed class RingGridMatcher : IDriverMatcher
{
    private readonly DriverStore _store;

    public RingGridMatcher(int n, int m) => _store = new DriverStore(n, m);

    public void UpsertDriver(int id, int x, int y) => _store.Upsert(id, x, y);
    public bool RemoveDriver(int id) => _store.Remove(id);

    public IReadOnlyList<NearestResult> FindNearest5(int orderX, int orderY)
    {
        var top = new Top5();
        var grid = _store.Grid;
        int n = _store.N, m = _store.M;

        for (int r = 0; r <= Math.Max(n, m); r++)
        {
            int minX = Math.Max(0, orderX - r);
            int maxX = Math.Min(n - 1, orderX + r);
            int minY = Math.Max(0, orderY - r);
            int maxY = Math.Min(m - 1, orderY + r);

            for (int x = minX; x <= maxX; x++)
            {
                TryCell(x, minY);
                if (maxY != minY) TryCell(x, maxY);
            }

            for (int y = minY + 1; y <= maxY - 1; y++)
            {
                TryCell(minX, y);
                if (maxX != minX) TryCell(maxX, y);
            }

            long worst = top.WorstDist2OrMax();
            long r2 = (long)r * r;
            if (top.Count == 5 && r2 > worst) break;
        }

        return top.ToArraySorted();

        void TryCell(int x, int y)
        {
            int id = grid[x, y];
            if (id == -1) return;

            if (!_store.DriversById.TryGetValue(id, out var pos))
                return;

            long d2 = Distance.SquaredEuclidean(orderX, orderY, pos.X, pos.Y);
            top.Add(new NearestResult(id, pos.X, pos.Y, d2));
        }
    }
}
