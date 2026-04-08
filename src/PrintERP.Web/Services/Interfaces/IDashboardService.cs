using PrintERP.Web.Models.ViewModels;

namespace PrintERP.Web.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardAsync(string role, DashboardFilterViewModel filter, CancellationToken cancellationToken = default);
}
