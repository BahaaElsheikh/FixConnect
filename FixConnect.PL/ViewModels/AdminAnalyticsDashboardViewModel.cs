using System.Collections.Generic;

namespace FixConnect.PL.ViewModels.Admin
{
    public class AdminAnalyticsDashboardViewModel
    {
        public AdminKpiViewModel Kpis { get; set; } = new AdminKpiViewModel();
        public List<RequestsRevenueByPeriodViewModel> RequestsRevenueOverTime { get; set; } = new List<RequestsRevenueByPeriodViewModel>();
        public List<SpecialtyDemandViewModel> TopSpecialties { get; set; } = new List<SpecialtyDemandViewModel>();
        public List<StatusDistributionViewModel> StatusDistribution { get; set; } = new List<StatusDistributionViewModel>();
        public UserRoleDistributionViewModel UserRoleDistribution { get; set; } = new UserRoleDistributionViewModel();
        public List<RegionDemandViewModel> TopRegions { get; set; } = new List<RegionDemandViewModel>();
    }
}