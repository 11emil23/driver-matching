using BenchmarkDotNet.Attributes;
using DriverMatching.Core;

namespace DriverMatching.Benchmarks;

[MemoryDiagnoser]
public class NearestBenchmarks
{
    private const int N = 2000;
    private const int M = 2000;

    // smaller default for quicker run in Codespaces; you can increase if you want
    private const int Drivers = 100_000;

    private IDriverMatcher _full = default!;
    private IDriverMatcher _ring = default!;
    private IDriverMatcher _bucket = default!;

    private (int X, int Y)[] _orders = default!;
    private int _idx;

    [GlobalSetup]
    public void Setup()
    {
        _full = new FullScanMatcher(N, M);
        _ring = new RingGridMatcher(N, M);
        _bucket = new BucketGridMatcher(N, M, bucketSize: 32);

        var rnd = new Random(42);
        var used = new HashSet<(int,int)>();

        for (int id = 1; id <= Drivers; id++)
        {
            int x, y;
            do
            {
                x = rnd.Next(N);
                y = rnd.Next(M);
            } while (!used.Add((x,y)));

            _full.UpsertDriver(id, x, y);
            _ring.UpsertDriver(id, x, y);
            _bucket.UpsertDriver(id, x, y);
        }

        _orders = new (int X, int Y)[2000];
        for (int i = 0; i < _orders.Length; i++)
            _orders[i] = (rnd.Next(N), rnd.Next(M));
    }

    private (int X, int Y) NextOrder()
    {
        var o = _orders[_idx++];
        if (_idx >= _orders.Length) _idx = 0;
        return o;
    }

    [Benchmark(Baseline = true)]
    public int FullScan()
    {
        var (x, y) = NextOrder();
        return _full.FindNearest5(x, y).Count;
    }

    [Benchmark]
    public int RingGrid()
    {
        var (x, y) = NextOrder();
        return _ring.FindNearest5(x, y).Count;
    }

    [Benchmark]
    public int BucketGrid()
    {
        var (x, y) = NextOrder();
        return _bucket.FindNearest5(x, y).Count;
    }
}
