

namespace FixConnect.BLL.DTOs
{
    public class AdminKpiViewModel
    {
        public int TotalRequestsThisMonth { get; set; }
        public decimal TotalRequestsGrowthPercent { get; set; }
        public decimal TotalRevenueThisMonth { get; set; }
        public decimal RevenueGrowthPercent { get; set; }
        public int NewUsersThisMonth { get; set; }
        public decimal CompletionRatePercent { get; set; }
    }

    public class RequestsRevenueByPeriodViewModel
    {
        public string PeriodLabel { get; set; }   // "Jan 2026"
        public int RequestsCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class SpecialtyDemandViewModel
    {
        public string SpecialtyName { get; set; }
        public int RequestCount { get; set; }
    }

    public class StatusDistributionViewModel
    {
        public string Status { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class UserRoleDistributionViewModel
    {
        public int CustomersCount { get; set; }
        public int WorkersCount { get; set; }
        public int VerifiedWorkersCount { get; set; }
        public int PendingWorkersCount { get; set; }
    }

    public class RegionDemandViewModel
    {
        public string RegionName { get; set; }
        public int RequestCount { get; set; }
    }
}