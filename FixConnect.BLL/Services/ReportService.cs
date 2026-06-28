using FixConnect.DAL.Context;
using FixConnect.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FixConnect.BLL.Services
{
    public class ReportService
    {
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────
        // Create New Report
        // ─────────────────────────────
        public (bool Success, string Message) CreateReport(Report report)
        {
            _context.Reports.Add(report);
            _context.SaveChanges();
            return (true, "Your report has been submitted successfully.");
        }

        // ─────────────────────────────
        // Toggle Resolve Status (Admin)
        // ─────────────────────────────
        public (bool Success, string Message) ToggleResolveStatus(int reportId, bool isResolved)
        {
            var report = _context.Reports.Find(reportId);
            if (report == null)
                return (false, "Report not found.");

            report.IsResolved = isResolved;
            _context.SaveChanges();

            string statusText = isResolved ? "resolved" : "reopened";
            return (true, $"Report marked as {statusText}.");
        }

        // ─────────────────────────────
        // Get Filtered & Sorted Reports Matrix (Admin Dashboard)
        // ─────────────────────────────
        public (List<Report> Items, int TotalCount) GetFilteredReports(
            string? search, string? category, string sortOrder, int pageIndex, int pageSize)
        {
            var query = _context.Reports
                .Include(r => r.Reporter)
                .AsNoTracking()
                .AsQueryable();

            // 1. Context Category Filtering
            if (!string.IsNullOrEmpty(category))
            {
                query = category switch
                {
                    "Job" => query.Where(r => r.JobId != null),
                    "Request" => query.Where(r => r.RequestId != null),
                    "Proposal" => query.Where(r => r.ProposalId != null),
                    "Review" => query.Where(r => r.ReviewId != null),
                    _ => query
                };
            }

            // 2. Search Parameters
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.Description.Contains(search) ||
                                         r.Reporter.FullName.Contains(search) ||
                                         r.Reporter.Email.Contains(search));
            }

            // 3. Sorting Mechanics
            query = sortOrder switch
            {
                "date_asc" => query.OrderBy(r => r.CreatedAt),
                _ => query.OrderByDescending(r => r.CreatedAt) // Default Execution: date_desc
            };

            // 4. Extract Total Records count before slicing execution
            int totalCount = query.Count();

            // 5. Execution of Pagination offset segments
            var items = query.Skip((pageIndex - 1) * pageSize)
                             .Take(pageSize)
                             .ToList();

            return (items, totalCount);
        }

        // ─────────────────────────────
        // Fetch Context Entity Meta for Submission Form
        // ─────────────────────────────
        public object? GetReportContextData(string category, int targetId)
        {
            if (targetId <= 0) return null;

            return category switch
            {
                "Job" => _context.Jobs
                    .Where(j => j.JobId == targetId)
                    .Select(j => new { ContextTitle = "Job Base Reference #" + j.JobId, JobId = (int?)targetId, RequestId = (int?)null, ProposalId = (int?)null, ReviewId = (int?)null, CustomerId = (int?)j.Proposal.UserId, WorkerId = (int?)j.Proposal.WorkerId, Category = "Job" })
                    .FirstOrDefault(),

                "Request" => _context.Requests
                    .Where(r => r.RequestId == targetId)
                    .Select(r => new { ContextTitle = "Request Title: " + r.Title, JobId = (int?)null, RequestId = (int?)targetId, ProposalId = (int?)null, ReviewId = (int?)null, CustomerId = (int?)r.UserId, WorkerId = r.TargetWorkerId, Category = "Request" })
                    .FirstOrDefault(),

                "Proposal" => _context.Proposals
                    .Where(p => p.ProposalId == targetId)
                    .Select(p => new { ContextTitle = "Proposal Labor Budget Estimation: $" + p.LaborCost, JobId = (int?)null, RequestId = (int?)null, ProposalId = (int?)targetId, ReviewId = (int?)null, CustomerId = (int?)p.UserId, WorkerId = (int?)p.WorkerId, Category = "Proposal" })
                    .FirstOrDefault(),

                "Worker" => _context.Users
                .Where(u => u.UserId == targetId)
                .Select(u => new { ContextTitle = "Report Profile for Worker: " + u.FullName, JobId = (int?)null, RequestId = (int?)null, ProposalId = (int?)null, ReviewId = (int?)null, CustomerId = (int?)null, WorkerId = (int?)targetId, Category = "Worker" })
                .FirstOrDefault(),


            "Customer" => _context.Users
                    .Where(u => u.UserId == targetId)
                    .Select(u => new { ContextTitle = "Report Profile for Worker: " + u.FullName, JobId = (int?)null, RequestId = (int?)null, ProposalId = (int?)null, ReviewId = (int?)null, CustomerId = (int?)null, WorkerId = (int?)targetId, Category = "Customer" })
                    .FirstOrDefault(),


                "Review" => _context.Reviews
                    .Where(r => r.ReviewId == targetId)
                    .Select(r => new { ContextTitle = "Review Feedback Content Snippet: \"" + r.Comment + "\"", JobId = (int?)null, RequestId = (int?)null, ProposalId = (int?)null, ReviewId = (int?)targetId, CustomerId = (int?)r.UserId, WorkerId = (int?)r.WorkerId, Category = "Review" })
                    .FirstOrDefault(),

                _ => null
            };
        }
    }
}