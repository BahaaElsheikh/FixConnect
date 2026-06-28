using FixConnect.BLL.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FixConnect.PL.ViewComponents
{
    public class NotificationBadgesViewComponent : ViewComponent
    {
        private readonly NotificationBadgeService _badgeService;

        public NotificationBadgesViewComponent(NotificationBadgeService badgeService)
        {
            _badgeService = badgeService;
        }

        // section: "DirectRequests" | "Proposals" | "Jobs" | "Wallet"
        public IViewComponentResult Invoke(string section, string elementId)
        {
            var userIdClaim = ((ClaimsPrincipal)User).FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Content(string.Empty);

            int workerId = int.Parse(userIdClaim.Value);
            var counts = _badgeService.GetBadgeCounts(workerId);

            int count = section switch
            {
                "DirectRequests" => counts.DirectRequests,
                "Proposals" => counts.Proposals,
                "Jobs" => counts.Jobs,
                "Wallet" => counts.Wallet,
                _ => 0
            };

            ViewData["Count"] = count;
            ViewData["ElementId"] = elementId;

            return View(counts);
        }
    }
}