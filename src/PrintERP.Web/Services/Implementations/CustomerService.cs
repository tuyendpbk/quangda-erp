using Microsoft.EntityFrameworkCore;
using PrintERP.Web.Data;
using PrintERP.Web.Data.Entities;
using PrintERP.Web.Models.ViewModels;
using PrintERP.Web.Services.Interfaces;

namespace PrintERP.Web.Services.Implementations;

public class CustomerService(AppDbContext dbContext) : ICustomerService
{
    public Task<List<CustomerSummaryViewModel>> GetCustomersAsync(CancellationToken cancellationToken = default)
        => dbContext.Customers.AsNoTracking().OrderBy(x => x.Name)
            .Select(x => new CustomerSummaryViewModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Phone = x.Phone,
                Email = x.Email,
                Address = x.Address,
                TaxCode = x.TaxCode,
                CurrentDebt = x.CurrentDebt
            }).ToListAsync(cancellationToken);

    public Task<CustomerSummaryViewModel?> GetCustomerByIdAsync(long id, CancellationToken cancellationToken = default)
        => dbContext.Customers.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new CustomerSummaryViewModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Phone = x.Phone,
                Email = x.Email,
                Address = x.Address,
                TaxCode = x.TaxCode,
                CurrentDebt = x.CurrentDebt
            }).FirstOrDefaultAsync(cancellationToken);

    public async Task<CustomerSummaryViewModel> QuickCreateAsync(CustomerQuickCreateViewModel model, CancellationToken cancellationToken = default)
    {
        var entity = new ErpCustomer
        {
            Code = $"CUS{DateTime.UtcNow:yyMMddHHmmss}",
            Name = model.Name.Trim(),
            Phone = model.Phone.Trim(),
            Email = model.Email?.Trim(),
            Address = model.Address?.Trim(),
            TaxCode = model.TaxCode?.Trim(),
            CurrentDebt = 0
        };

        dbContext.Customers.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CustomerSummaryViewModel
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Phone = entity.Phone,
            Email = entity.Email,
            Address = entity.Address,
            TaxCode = entity.TaxCode,
            CurrentDebt = entity.CurrentDebt
        };
    }
}
