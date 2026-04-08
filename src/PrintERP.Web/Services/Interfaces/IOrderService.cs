using System.Security.Claims;
using PrintERP.Web.Models.ViewModels;

namespace PrintERP.Web.Services.Interfaces;

public interface IOrderService
{
    Task<OrderCreatePageViewModel> BuildCreatePageAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error, long? OrderId)> CreateAsync(OrderCreateViewModel model, CancellationToken cancellationToken = default);
    Task<OrderDetailViewModel?> GetDetailAsync(long id, ClaimsPrincipal user, CancellationToken cancellationToken = default);
}
