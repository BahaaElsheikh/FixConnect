using FixConnect.BLL.Services;
using FixConnect.DAL.Models;
using FixConnect.PL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace FixConnect.PL.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ReportService _reportService;

        public ReportsController(ReportService reportService)
        {
            _reportService = reportService;
        }

        // GET: Reports/Create
        // GET: Reports/Create
        [HttpGet]
        public IActionResult Create(string category, int targetId)
        {
            if (string.IsNullOrEmpty(category)) return BadRequest();

            var viewModel = new SubmitReportViewModel
            {
                Category = category,
                ContextTitle = $"Report {category} (ID: {targetId})"
            };

            // بنربط الـ ID بالـ Property الصح على حسب الـ Category المبعوتة
            if (category.Equals("Worker", StringComparison.OrdinalIgnoreCase))
            {
                viewModel.WorkerId = targetId;
                viewModel.ContextTitle = "Report Worker Profile";
            }
            else if (category.Equals("Customer", StringComparison.OrdinalIgnoreCase))
            {
                viewModel.CustomerId = targetId;
                viewModel.ContextTitle = "Report Customer Profile";
            }

            // هنا بنحاول نلقط أي داتا إضافية من الـ Service بأمان بدون ما يضرب Runtime error
            try
            {
                var contextData = _reportService.GetReportContextData(category, targetId);
                if (contextData != null)
                {
                    // لو الـ Service مرجعة داتا، بنحاول نقرأ الـ Properties المشتركة بحرص
                    var properties = contextData.GetType().GetProperties();

                    foreach (var prop in properties)
                    {
                        var val = prop.GetValue(contextData);
                        if (val == null) continue;

                        switch (prop.Name)
                        {
                            case "ContextTitle": viewModel.ContextTitle = val.ToString(); break;
                            case "JobId": viewModel.JobId = (int?)val; break;
                            case "RequestId": viewModel.RequestId = (int?)val; break;
                            case "ProposalId": viewModel.ProposalId = (int?)val; break;
                            case "ReviewId": viewModel.ReviewId = (int?)val; break;
                        }
                    }
                }
            }
            catch
            {
                // لو الـ Service ضربت لأي سبب، الـ Catch هتحمي الأبليكيشن والصفحة هتفتح برضه بالبيانات الأساسية فوق
            }

            return View(viewModel);
        }

        // POST: Reports/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(SubmitReportViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(nameIdentifierClaim)) return Unauthorized();

            var newReport = new Report
            {
                ReporterId = int.Parse(nameIdentifierClaim),
                JobId = model.JobId,
                RequestId = model.RequestId,
                ProposalId = model.ProposalId,
                ReviewId = model.ReviewId,
                CustomerId = model.CustomerId,
                WorkerId = model.WorkerId,
                Description = model.Description,
                CreatedAt = DateTime.Now,
                IsResolved = false
            };

            var result = _reportService.CreateReport(newReport);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Processing database transaction failure encountered.");
            return View(model);
        }

        // GET: Reports/AdminIndex
        public IActionResult AdminIndex(string? search, string? categoryFilter, string sortOrder = "date_desc", int pageNumber = 1)
        {
            const int defaultPageSize = 10;

            var result = _reportService.GetFilteredReports(search, categoryFilter, sortOrder, pageNumber, defaultPageSize);

            var paginatedList = new PaginatedList<Report>
            {
                Items = result.Items,
                PageIndex = pageNumber,
                TotalPages = (int)Math.Ceiling(result.TotalCount / (double)defaultPageSize),
                TotalCount = result.TotalCount
            };

            var reportDashboardModel = new ReportsListViewModel
            {
                Reports = paginatedList,
                SearchQuery = search,
                CategoryFilter = categoryFilter,
                SortOrder = sortOrder
            };

            return View(reportDashboardModel);
        }

        // POST: Reports/ToggleResolve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleResolve(int reportId, bool currentStatus, string? search, string? categoryFilter, string sortOrder)
        {
            _reportService.ToggleResolveStatus(reportId, currentStatus);
            return RedirectToAction(nameof(AdminIndex), new { search, categoryFilter, sortOrder });
        }
    }
}