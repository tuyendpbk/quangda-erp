using System.Security.Claims;
using PrintERP.Web.Models.ViewModels;

namespace PrintERP.Web.Services.Interfaces;

public interface IEmployeeService
{
    Task<EmployeeListViewModel> GetListAsync(EmployeeFilterViewModel filter, ClaimsPrincipal user, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> ToggleStatusAsync(long id, ClaimsPrincipal user, CancellationToken cancellationToken = default);
    Task<EmployeeCreatePageViewModel> BuildCreatePageAsync(EmployeeCreateViewModel? input = null, CancellationToken cancellationToken = default);
    Task<EmployeeCreateResult> CreateAsync(EmployeeCreateViewModel model, ClaimsPrincipal user, CancellationToken cancellationToken = default);
}
