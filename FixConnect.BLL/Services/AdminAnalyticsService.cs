using System;
using System.Collections.Generic;
using System.Linq;

using FixConnect.DAL.Context;
using FixConnect.DAL.Data.Enums;
using FixConnect.BLL.DTOs;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FixConnect.BLL.Services
{
    public class AdminAnalyticsService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        private const string CacheKey = "AdminAnalyticsDashboard";
        private const int CacheDurationMinutes = 5;

        // ✅ Dependency Injection: AppDbContext + IMemoryCache injected via constructor
        public AdminAnalyticsService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public AdminAnalyticsDashboardViewModel GetFullDashboardData()
        {
            if (_cache.TryGetValue(CacheKey, out AdminAnalyticsDashboardViewModel cachedData))
            {
                return cachedData;
            }

            var freshData = new AdminAnalyticsDashboardViewModel
            {
                Kpis = BuildKpis(),
                RequestsRevenueOverTime = BuildRequestsRevenueOverTime(),
                TopSpecialties = BuildTopSpecialties(),
                StatusDistribution = BuildStatusDistribution(),
                UserRoleDistribution = BuildUserRoleDistribution(),
                TopRegions = BuildTopRegions()
            };

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheDurationMinutes));

            _cache.Set(CacheKey, freshData, cacheOptions);

            return freshData;
        }

        // ============================
        // KPIs
        // ============================
        private AdminKpiViewModel BuildKpis()
        {
            var now = DateTime.Now;
            var startOfThisMonth = new DateTime(now.Year, now.Month, 1);
            var startOfLastMonth = startOfThisMonth.AddMonths(-1);
            var startOfNextMonth = startOfThisMonth.AddMonths(1);

            // Requests this month vs last month
            int requestsThisMonth = _context.Requests
                .Count(r => r.CreatedAt >= startOfThisMonth && r.CreatedAt < startOfNextMonth);

            int requestsLastMonth = _context.Requests
                .Count(r => r.CreatedAt >= startOfLastMonth && r.CreatedAt < startOfThisMonth);

            decimal requestsGrowth = CalculateGrowthPercent(requestsLastMonth, requestsThisMonth);

            // Revenue this month vs last month (10% commission on LaborCost, only Completed jobs)
            decimal revenueThisMonth = _context.Jobs
                .Where(j => j.Status == JobStatus.Completed
                         && j.CreatedAt >= startOfThisMonth && j.CreatedAt < startOfNextMonth
                         && j.LaborCost.HasValue)
                .Sum(j => j.LaborCost.Value * 0.10m);

            decimal revenueLastMonth = _context.Jobs
                .Where(j => j.Status == JobStatus.Completed
                         && j.CreatedAt >= startOfLastMonth && j.CreatedAt < startOfThisMonth
                         && j.LaborCost.HasValue)
                .Sum(j => j.LaborCost.Value * 0.10m);

            decimal revenueGrowth = CalculateGrowthPercent((double)revenueLastMonth, (double)revenueThisMonth);

            // New users this month (Customers + Workers, excluding Admins)
            int newUsersThisMonth = _context.Users
                .Count(u => u.CreatedAt >= startOfThisMonth && u.CreatedAt < startOfNextMonth
                         && (u.RoleType == RoleType.Customer || u.RoleType == RoleType.Worker));

            // Completion rate = Completed Jobs / Total Requests (all-time)
            int totalRequests = _context.Requests.Count();
            int completedJobsCount = _context.Jobs.Count(j => j.Status == JobStatus.Completed);

            decimal completionRate = totalRequests == 0
                ? 0
                : Math.Round((decimal)completedJobsCount / totalRequests * 100, 2);

            return new AdminKpiViewModel
            {
                TotalRequestsThisMonth = requestsThisMonth,
                TotalRequestsGrowthPercent = requestsGrowth,
                TotalRevenueThisMonth = Math.Round(revenueThisMonth, 2),
                RevenueGrowthPercent = revenueGrowth,
                NewUsersThisMonth = newUsersThisMonth,
                CompletionRatePercent = completionRate
            };
        }

        private decimal CalculateGrowthPercent(double previous, double current)
        {
            if (previous == 0)
                return current == 0 ? 0 : 100;

            return Math.Round((decimal)((current - previous) / previous * 100), 2);
        }

        // ============================
        // Chart 1: Requests & Revenue Over Time (last 6 months)
        // ============================
        private List<RequestsRevenueByPeriodViewModel> BuildRequestsRevenueOverTime()
        {
            const int monthsBack = 6;
            var now = DateTime.Now;
            var result = new List<RequestsRevenueByPeriodViewModel>();

            for (int i = monthsBack - 1; i >= 0; i--)
            {
                var periodStart = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
                var periodEnd = periodStart.AddMonths(1);

                int requestsCount = _context.Requests
                    .Count(r => r.CreatedAt >= periodStart && r.CreatedAt < periodEnd);

                decimal revenue = _context.Jobs
                    .Where(j => j.Status == JobStatus.Completed
                             && j.CreatedAt >= periodStart && j.CreatedAt < periodEnd
                             && j.LaborCost.HasValue)
                    .Sum(j => j.LaborCost.Value * 0.10m);

                result.Add(new RequestsRevenueByPeriodViewModel
                {
                    PeriodLabel = periodStart.ToString("MMM yyyy"),
                    RequestsCount = requestsCount,
                    Revenue = Math.Round(revenue, 2)
                });
            }

            return result;
        }

        // ============================
        // Chart 2: Top Requested Specialties
        // ============================
        private List<SpecialtyDemandViewModel> BuildTopSpecialties(int topN = 5)
        {
            return _context.Requests
                .Where(r => r.SpecialtyId != null)
                .GroupBy(r => r.Specialty!.SpecialtyName)
                .Select(g => new SpecialtyDemandViewModel
                {
                    SpecialtyName = g.Key,
                    RequestCount = g.Count()
                })
                .OrderByDescending(x => x.RequestCount)
                .Take(topN)
                .ToList();
        }

        // ============================
        // Chart 3: Request Status Distribution
        // ============================
        private List<StatusDistributionViewModel> BuildStatusDistribution()
        {
            int totalRequests = _context.Requests.Count();

            var grouped = _context.Requests
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            return grouped.Select(g => new StatusDistributionViewModel
            {
                Status = ((RequestStatus)g.Status).ToString(),
                Count = g.Count,
                Percentage = totalRequests == 0 ? 0 : Math.Round((decimal)g.Count / totalRequests * 100, 2)
            }).ToList();
        }

        // ============================
        // Chart 4: User Role Distribution
        // ============================
        private UserRoleDistributionViewModel BuildUserRoleDistribution()
        {
            int customersCount = _context.Customers.Count();
            int workersCount = _context.Workers.Count();
            int verifiedWorkersCount = _context.Workers.Count(w => w.IsVerified);
            int pendingWorkersCount = workersCount - verifiedWorkersCount;

            return new UserRoleDistributionViewModel
            {
                CustomersCount = customersCount,
                WorkersCount = workersCount,
                VerifiedWorkersCount = verifiedWorkersCount,
                PendingWorkersCount = pendingWorkersCount
            };
        }

        // ============================
        // Chart 5: Top Performing Regions (Governorate-level)
        // ============================
        private List<RegionDemandViewModel> BuildTopRegions(int topN = 5)
        {
            // Region is stored as a free-text string "Governorate, City, Area"
            // We pull RegionName values into memory then group by the governorate (first comma segment)
            var regionNames = _context.Requests
                .Select(r => r.Region.RegionName)
                .ToList();

            return regionNames
                .Select(name => string.IsNullOrWhiteSpace(name)
                    ? "Unknown"
                    : name.Split(',')[0].Trim())
                .GroupBy(governorate => governorate)
                .Select(g => new RegionDemandViewModel
                {
                    RegionName = g.Key,
                    RequestCount = g.Count()
                })
                .OrderByDescending(x => x.RequestCount)
                .Take(topN)
                .ToList();
        }
    }
}