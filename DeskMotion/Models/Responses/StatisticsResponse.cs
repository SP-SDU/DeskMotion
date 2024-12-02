namespace DeskMotion.Models.Responses;

public class StatisticsResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public DeskUsageStatistics? Statistics { get; set; }

    public static StatisticsResponse FromSuccess(DeskUsageStatistics statistics)
    {
        return new StatisticsResponse
        {
            Success = true,
            Statistics = statistics
        };
    }

    public static StatisticsResponse FromError(string error)
    {
        return new StatisticsResponse
        {
            Success = false,
            Error = error
        };
    }
}

public class UsageRecordResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }

    public static UsageRecordResponse FromSuccess()
    {
        return new UsageRecordResponse { Success = true };
    }

    public static UsageRecordResponse FromError(string error)
    {
        return new UsageRecordResponse
        {
            Success = false,
            Error = error
        };
    }
}
