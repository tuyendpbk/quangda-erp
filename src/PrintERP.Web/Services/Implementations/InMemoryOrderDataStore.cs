using PrintERP.Web.Models.ViewModels;

namespace PrintERP.Web.Services.Implementations;

public static class InMemoryOrderDataStore
{
    private static long _customerSeq = 1003;
    private static long _orderSeq = 2000;

    public static List<CustomerSummaryViewModel> Customers { get; } =
    [
        new() { Id = 1001, Code = "CUS001001", Name = "Công ty Minh Tâm", Phone = "0909123456", Email = "contact@minhtam.vn", Address = "Q1, TP.HCM", TaxCode = "0312345678", CurrentDebt = 12500000 },
        new() { Id = 1002, Code = "CUS001002", Name = "Shop Nắng Mai", Phone = "0988123456", Email = "hello@nangmai.vn", Address = "Bình Thạnh, TP.HCM", TaxCode = "0311112222", CurrentDebt = 0 },
        new() { Id = 1003, Code = "CUS001003", Name = "Anh Lê Hùng", Phone = "0911122233", Email = "", Address = "Thủ Đức, TP.HCM", TaxCode = "", CurrentDebt = 3400000 }
    ];

    public static List<EmployeeOptionViewModel> Employees { get; } =
    [
        new() { Id = 1, Name = "System Administrator" },
        new() { Id = 2, Name = "Factory Manager" },
        new() { Id = 3, Name = "Sales User" },
        new() { Id = 4, Name = "Warehouse User" }
    ];

    public static List<ProductCategoryOptionViewModel> ProductCategories { get; } =
    [
        new() { Id = 1, Name = "In bạt", DefaultUnit = "M2", UseAreaPricing = true },
        new() { Id = 2, Name = "In decal", DefaultUnit = "M2", UseAreaPricing = true },
        new() { Id = 3, Name = "Bảng alu", DefaultUnit = "TẤM", UseAreaPricing = false },
        new() { Id = 4, Name = "Mica", DefaultUnit = "TẤM", UseAreaPricing = false },
        new() { Id = 5, Name = "Name card", DefaultUnit = "HỘP", UseAreaPricing = false },
        new() { Id = 6, Name = "Tờ rơi", DefaultUnit = "XẤP", UseAreaPricing = false }
    ];

    public static List<object> Orders { get; } = [];

    public static string NextOrderCode()
    {
        _orderSeq++;
        return $"ORD{DateTime.UtcNow:yyyy}{_orderSeq:000}";
    }

    public static long NextOrderId() => _orderSeq;

    public static (long id, string code) AddCustomer(CustomerQuickCreateViewModel model)
    {
        _customerSeq++;
        var id = _customerSeq;
        var code = $"CUS{id:000000}";
        Customers.Add(new CustomerSummaryViewModel
        {
            Id = id,
            Code = code,
            Name = model.Name.Trim(),
            Phone = model.Phone.Trim(),
            Email = model.Email?.Trim(),
            Address = model.Address?.Trim(),
            TaxCode = model.TaxCode?.Trim(),
            CurrentDebt = 0
        });

        return (id, code);
    }
}
