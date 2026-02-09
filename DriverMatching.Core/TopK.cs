namespace DriverMatching.Core;

internal struct Top5
{
    private const int K = 5;
    private int _count;

    private NearestResult _a0, _a1, _a2, _a3, _a4;

    public int Count => _count;

    public void Add(NearestResult r)
    {
        if (_count < K)
        {
            Set(_count, r);
            _count++;
            BubbleUp(_count - 1);
            return;
        }

        var worst = Get(K - 1);
        if (Compare(r, worst) >= 0)
            return;

        Set(K - 1, r);
        BubbleUp(K - 1);
    }

    public long WorstDist2OrMax()
    {
        if (_count < K) return long.MaxValue;
        return Get(K - 1).Dist2;
    }

    public NearestResult[] ToArraySorted()
    {
        var res = new NearestResult[_count];
        for (int i = 0; i < _count; i++)
            res[i] = Get(i);
        return res;
    }

    private void BubbleUp(int idx)
    {
        while (idx > 0)
        {
            int prev = idx - 1;
            var cur = Get(idx);
            var p = Get(prev);

            if (Compare(cur, p) >= 0) break;

            Set(idx, p);
            Set(prev, cur);
            idx = prev;
        }
    }

    private static int Compare(in NearestResult a, in NearestResult b)
    {
        int d = a.Dist2.CompareTo(b.Dist2);
        return d != 0 ? d : a.DriverId.CompareTo(b.DriverId);
    }

    private NearestResult Get(int i) => i switch
    {
        0 => _a0, 1 => _a1, 2 => _a2, 3 => _a3, 4 => _a4,
        _ => throw new ArgumentOutOfRangeException()
    };

    private void Set(int i, NearestResult v)
    {
        switch (i)
        {
            case 0: _a0 = v; break;
            case 1: _a1 = v; break;
            case 2: _a2 = v; break;
            case 3: _a3 = v; break;
            case 4: _a4 = v; break;
            default: throw new ArgumentOutOfRangeException();
        }
    }
}
