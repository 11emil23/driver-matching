using System.Collections.Generic;

namespace DriverMatching.Core;

/// <summary>
/// Algorithm 3: Spatial hashing with fixed-size buckets (bucketSize x bucketSize).
/// Query expands by bucket rings. Early stop uses exact lower bound over the next ring perimeter.
/// </summary>
public sealed class BucketGridMatcher : IDriverMatcher
{
    private readonly DriverStore _store;
    private readonly int _bucketSize;

    private readonly Dictionary<(int, int), List<int>> _buckets = new();
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

        _store.Upsert(id, x, y);

        var newB = GetBucket(x, y);

        if (!existed)
        {
            AddToBucket(id, newB.Item1, newB.Item2);
            _driverBucket[id] = newB;
            return;
        }

        var oldB = _driverBucket[id];
        if (oldB != newB)
        {
            RemoveFromBucket(id, oldB.Item1, oldB.Item2);
            AddToBucket(id, newB.Item1, newB.Item2);
            _driverBucket[id] = newB;
        }
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

        int n = _store.N, m = _store.M;
        int maxBx = (n - 1) / _bucketSize;
        int maxBy = (m - 1) / _bucketSize;

        var orderB = GetBucket(orderX, orderY);
        int obx = orderB.Item1, oby = orderB.Item2;

        for (int r = 0; r <= Math.Max(maxBx, maxBy); r++)
        {
            int minBx = Math.Max(0, obx - r);
            int maxBxR = Math.Min(maxBx, obx + r);
            int minBy = Math.Max(0, oby - r);
            int maxByR = Math.Min(maxBy, oby + r);

            for (int bx = minBx; bx <= maxBxR; bx++)
            {
                ScanBucket(bx, minBy);
                if (maxByR != minBy) ScanBucket(bx, maxByR);
            }

            for (int by = minBy + 1; by <= maxByR - 1; by++)
            {
                ScanBucket(minBx, by);
                if (maxBxR != minBx) ScanBucket(maxBxR, by);
            }

            if (top.Count == 5)
            {
                long worst = top.WorstDist2OrMax();
                long minPossibleNext = MinPossibleDist2ToBucketRing(orderX, orderY, obx, oby, r + 1, maxBx, maxBy);
                if (minPossibleNext > worst) break;
            }
        }

        return top.ToArraySorted();

        void ScanBucket(int bx, int by)
        {
            if (!_buckets.TryGetValue((bx, by), out var list)) return;

            foreach (int id in list)
            {
                if (!_store.DriversById.TryGetValue(id, out var pos)) continue;
                long d2 = Distance.SquaredEuclidean(orderX, orderY, pos.X, pos.Y);
                top.Add(new NearestResult(id, pos.X, pos.Y, d2));
            }
        }
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

    private long MinPossibleDist2ToBucketRing(int x, int y, int obx, int oby, int ringR, int maxBx, int maxBy)
    {
        int minBx = Math.Max(0, obx - ringR);
        int maxBxR = Math.Min(maxBx, obx + ringR);
        int minBy = Math.Max(0, oby - ringR);
        int maxByR = Math.Min(maxBy, oby + ringR);

        long best = long.MaxValue;

        for (int bx = minBx; bx <= maxBxR; bx++)
        {
            best = Math.Min(best, MinDist2ToBucket(x, y, bx, minBy));
            if (maxByR != minBy)
                best = Math.Min(best, MinDist2ToBucket(x, y, bx, maxByR));
        }

        for (int by = minBy + 1; by <= maxByR - 1; by++)
        {
            best = Math.Min(best, MinDist2ToBucket(x, y, minBx, by));
            if (maxBxR != minBx)
                best = Math.Min(best, MinDist2ToBucket(x, y, maxBxR, by));
        }

        return best;
    }

    private long MinDist2ToBucket(int x, int y, int bx, int by)
    {
        int x0 = bx * _bucketSize;
        int y0 = by * _bucketSize;
        int x1 = Math.Min(_store.N - 1, x0 + _bucketSize - 1);
        int y1 = Math.Min(_store.M - 1, y0 + _bucketSize - 1);

        int cx = x < x0 ? x0 : (x > x1 ? x1 : x);
        int cy = y < y0 ? y0 : (y > y1 ? y1 : y);

        return Distance.SquaredEuclidean(x, y, cx, cy);
    }
}
