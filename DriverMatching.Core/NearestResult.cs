namespace DriverMatching.Core;

public readonly record struct NearestResult(int DriverId, int X, int Y, long Dist2);
