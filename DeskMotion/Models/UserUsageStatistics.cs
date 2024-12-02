using System;
using System.Collections.Generic;

namespace DeskMotion.Models
{
    public class UserUsageStatistics
    {
        public Guid UserId { get; set; }
        public TimeSpan TotalUsageTime { get; set; }
        public int NumberOfAdjustments { get; set; }
        public List<DeskUsageSession> DeskUsageSessions { get; set; } = new();
    }

    public class DeskUsageSession
    {
        public string DeskId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
