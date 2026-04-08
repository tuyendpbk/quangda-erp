using PrintERP.Web.Models.ViewModels;

namespace PrintERP.Web.Services.Interfaces;

public interface ICustomerService
{
    Task<List<CustomerSummaryViewModel>> GetCustomersAsync(CancellationToken cancellationToken = default);
    Task<CustomerSummaryViewModel?> GetCustomerByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<CustomerSummaryViewModel> QuickCreateAsync(CustomerQuickCreateViewModel model, CancellationToken cancellationToken = default);
}
