namespace DriverMatching.Core;

/// <summary>Algorithm 1: Full scan baseline. Complexity O(D) per query.</summary>
public sealed class FullScanMatcher : IDriverMatcher
{
    private readonly DriverStore _store;

    public FullScanMatcher(int n, int m) => _store = new DriverStore(n, m);

    public void UpsertDriver(int id, int x, int y) => _store.Upsert(id, x, y);
    public bool RemoveDriver(int id) => _store.Remove(id);

    public IReadOnlyList<NearestResult> FindNearest5(int orderX, int orderY)
    {
        var top = new Top5();

        foreach (var kv in _store.DriversById)
        {
            int id = kv.Key;
            var (x, y) = kv.Value;
            long d2 = Distance.SquaredEuclidean(orderX, orderY, x, y);
            top.Add(new NearestResult(id, x, y, d2));
        }

        return top.ToArraySorted();
    }
}
