using PrintERP.Web.Models.ViewModels;
using PrintERP.Web.Services.Interfaces;

namespace PrintERP.Web.Services.Implementations;

public class CustomerService : ICustomerService
{
    public Task<List<CustomerSummaryViewModel>> GetCustomersAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(InMemoryOrderDataStore.Customers.OrderBy(x => x.Name).ToList());

    public Task<CustomerSummaryViewModel?> GetCustomerByIdAsync(long id, CancellationToken cancellationToken = default)
        => Task.FromResult(InMemoryOrderDataStore.Customers.FirstOrDefault(x => x.Id == id));

    public Task<CustomerSummaryViewModel> QuickCreateAsync(CustomerQuickCreateViewModel model, CancellationToken cancellationToken = default)
    {
        var (id, _) = InMemoryOrderDataStore.AddCustomer(model);
        var customer = InMemoryOrderDataStore.Customers.First(x => x.Id == id);
        return Task.FromResult(customer);
    }
}
