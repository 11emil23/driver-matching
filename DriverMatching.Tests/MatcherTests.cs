using DriverMatching.Core;
using NUnit.Framework;

namespace DriverMatching.Tests;

public class MatcherTests
{
    private static IDriverMatcher[] CreateAll(int n, int m) =>
    [
        new FullScanMatcher(n, m),
        new RingGridMatcher(n, m),
        new BucketGridMatcher(n, m, bucketSize: 8),
    ];

    [Test]
    public void Empty_ReturnsEmpty()
    {
        var matchers = CreateAll(10, 10);
        foreach (var m in matchers)
        {
            var res = m.FindNearest5(5, 5);
            Assert.That(res, Is.Empty);
        }
    }

    [Test]
    public void LessThan5_ReturnsAllSorted()
    {
        var matchers = CreateAll(10, 10);
        foreach (var m in matchers)
        {
            m.UpsertDriver(10, 1, 1);
            m.UpsertDriver(20, 2, 2);
            m.UpsertDriver(30, 3, 3);

            var res = m.FindNearest5(0, 0);
            Assert.That(res.Count, Is.EqualTo(3));
            AssertSorted(res);
        }
    }

    [Test]
    public void Deterministic_TieBreakById()
    {
        var matchers = CreateAll(5, 5);

        foreach (var m in matchers)
        {
            m.UpsertDriver(2, 1, 0);
            m.UpsertDriver(1, 0, 1);

            var res = m.FindNearest5(0, 0);
            Assert.That(res.Count, Is.EqualTo(2));
            Assert.That(res[0].DriverId, Is.EqualTo(1));
            Assert.That(res[1].DriverId, Is.EqualTo(2));
        }
    }

    [Test]
    public void OneDriverPerCell_IsEnforced()
    {
        var m = new FullScanMatcher(10, 10);
        m.UpsertDriver(1, 1, 1);
        Assert.Throws<InvalidOperationException>(() => m.UpsertDriver(2, 1, 1));
    }

    [Test]
    public void AllAlgorithms_MatchBaseline_OnRandomData()
    {
        const int n = 80, m = 80;
        var rnd = new Random(123);

        var baseline = new FullScanMatcher(n, m);
        var others = new IDriverMatcher[]
        {
            new RingGridMatcher(n, m),
            new BucketGridMatcher(n, m, bucketSize: 8),
        };

        var used = new HashSet<(int,int)>();

        for (int id = 1; id <= 500; id++)
        {
            int x, y;
            do
            {
                x = rnd.Next(n);
                y = rnd.Next(m);
            } while (!used.Add((x,y)));

            baseline.UpsertDriver(id, x, y);
            foreach (var o in others) o.UpsertDriver(id, x, y);
        }

        for (int t = 0; t < 200; t++)
        {
            int ox = rnd.Next(n);
            int oy = rnd.Next(m);

            var b = baseline.FindNearest5(ox, oy).ToArray();
            foreach (var o in others)
            {
                var r = o.FindNearest5(ox, oy).ToArray();
                Assert.That(r, Is.EqualTo(b));
            }
        }
    }

    private static void AssertSorted(IReadOnlyList<NearestResult> res)
    {
        for (int i = 1; i < res.Count; i++)
        {
            var a = res[i - 1];
            var b = res[i];

            if (a.Dist2 == b.Dist2)
                Assert.That(a.DriverId, Is.LessThanOrEqualTo(b.DriverId));
            else
                Assert.That(a.Dist2, Is.LessThanOrEqualTo(b.Dist2));
        }
    }
}
