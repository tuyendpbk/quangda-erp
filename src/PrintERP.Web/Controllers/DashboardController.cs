using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintERP.Web.Models.ViewModels;
using PrintERP.Web.Services.Interfaces;

namespace PrintERP.Web.Controllers;

[Authorize]
[Authorize(Policy = "DashboardView")]
public class DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] DashboardFilterViewModel filter, CancellationToken cancellationToken)
    {
        if (string.Equals(filter.PeriodType, DashboardPeriodTypes.Custom, StringComparison.OrdinalIgnoreCase)
            && filter.FromDate.HasValue
            && filter.ToDate.HasValue
            && filter.FromDate > filter.ToDate)
        {
            ModelState.AddModelError(nameof(filter.FromDate), "Từ ngày không được lớn hơn đến ngày");
            ModelState.AddModelError(nameof(filter.ToDate), "Đến ngày không hợp lệ");
        }

        try
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? DashboardRoles.Sales;
            var vm = await dashboardService.GetDashboardAsync(role, filter, cancellationToken);
            return View(vm);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Dashboard load failed for user {Username} at {TimestampUtc}.", User.Identity?.Name, DateTime.UtcNow);
            TempData["DashboardError"] = "Không thể tải dữ liệu dashboard. Vui lòng thử lại sau.";
            return View(new DashboardViewModel
            {
                Filter = filter
            });
        }
    }
}
