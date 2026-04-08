using PrintERP.Web.Models.ViewModels;

namespace PrintERP.Web.Services.Interfaces;

public interface IPricingService
{
    Task<OrderItemEstimateResultViewModel> EstimateItemAsync(OrderItemEstimateRequest request, CancellationToken cancellationToken = default);
}
