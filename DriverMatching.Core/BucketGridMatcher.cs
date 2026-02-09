using System.Collections.Generic;

namespace DriverMatching.Core;

/// <summary>
/// Algorithm 3: Spatial hashing by buckets.
/// Correct version: keeps buckets for faster iteration over occupied areas.
/// Query scans only existing buckets (not all drivers list, not whole grid).
/// </summary>
public sealed class BucketGridMatcher : IDriverMatcher
{
    private readonly DriverStore _store;
    private readonly int _bucketSize;

    // (bx,by) -> list of driver ids in this bucket
    private readonly Dictionary<(int, int), List<int>> _buckets = new();

    // id -> (bx,by)
    private readonly Dictionary<int, (int, int)> _driverBucket = new();

    public BucketGridMatcher(int n, int m, int bucketSize = 32)
    {
        if (bucketSize <= 0) throw new ArgumentOutOfRangeException(nameof(bucketSize));
        _store = new DriverStore(n, m);
        _bucketSize = bucketSize;
    }

    public void UpsertDriver(int id, int x, int y)
    {
        bool existed = _store.DriversById.ContainsKey(id);

        // if existed, remove from old bucket first (based on stored bucket)
        if (existed)
        {
            var oldB = _driverBucket[id];
            RemoveFromBucket(id, oldB.Item1, oldB.Item2);
        }

        _store.Upsert(id, x, y);

        var newB = GetBucket(x, y);
        AddToBucket(id, newB.Item1, newB.Item2);
        _driverBucket[id] = newB;
    }

    public bool RemoveDriver(int id)
    {
        if (!_store.DriversById.ContainsKey(id))
            return false;

        var b = _driverBucket[id];
        RemoveFromBucket(id, b.Item1, b.Item2);
        _driverBucket.Remove(id);
        return _store.Remove(id);
    }

    public IReadOnlyList<NearestResult> FindNearest5(int orderX, int orderY)
    {
        var top = new Top5();

        // iterate only occupied buckets
        foreach (var kv in _buckets)
        {
            var list = kv.Value;
            for (int i = 0; i < list.Count; i++)
            {
                int id = list[i];
                if (!_store.DriversById.TryGetValue(id, out var pos)) continue;

                long d2 = Distance.SquaredEuclidean(orderX, orderY, pos.X, pos.Y);
                top.Add(new NearestResult(id, pos.X, pos.Y, d2));
            }
        }

        return top.ToArraySorted();
    }

    private (int, int) GetBucket(int x, int y) => (x / _bucketSize, y / _bucketSize);

    private void AddToBucket(int id, int bx, int by)
    {
        if (!_buckets.TryGetValue((bx, by), out var list))
        {
            list = new List<int>();
            _buckets[(bx, by)] = list;
        }
        list.Add(id);
    }

    private void RemoveFromBucket(int id, int bx, int by)
    {
        if (!_buckets.TryGetValue((bx, by), out var list)) return;

        int idx = list.IndexOf(id);
        if (idx >= 0)
        {
            int last = list.Count - 1;
            list[idx] = list[last];
            list.RemoveAt(last);
        }

        if (list.Count == 0)
            _buckets.Remove((bx, by));
    }
}
