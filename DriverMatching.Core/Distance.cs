namespace DriverMatching.Core;

public static class Distance
{
    public static long SquaredEuclidean(int ax, int ay, int bx, int by)
    {
        long dx = ax - (long)bx;
        long dy = ay - (long)by;
        return dx * dx + dy * dy;
    }
}
