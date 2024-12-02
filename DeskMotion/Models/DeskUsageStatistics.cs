namespace DeskMotion.Models;

public class DeskUsageStatistics
{
    public string DeskId { get; set; } = string.Empty;
    public TimeSpan TotalUsageTime { get; set; }
    public int[] NumberOfAdjustments { get; set; } = new int[24];
    public List<UsageSession> UsageSessions { get; set; } = new();
}

public class UsageSession
{
    public Guid UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
