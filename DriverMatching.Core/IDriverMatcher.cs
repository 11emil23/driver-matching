namespace DriverMatching.Core;

public interface IDriverMatcher
{
    void UpsertDriver(int id, int x, int y);
    bool RemoveDriver(int id);

    /// <summary>Return up to 5 nearest drivers sorted by (Dist2, DriverId).</summary>
    IReadOnlyList<NearestResult> FindNearest5(int orderX, int orderY);
}
