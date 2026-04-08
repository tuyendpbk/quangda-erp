using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PrintERP.Web.Data.Entities;
using PrintERP.Web.Models.ViewModels;

namespace PrintERP.Web.Data;

public class DbSeedService(AppDbContext dbContext)
{
    private static readonly PasswordHasher<ErpEmployee> PasswordHasher = new();

    public async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (!await dbContext.Employees.AnyAsync(cancellationToken))
        {
            var employees = new[]
            {
                new ErpEmployee { Id = 1, Username = "admin", FullName = "System Administrator", Role = "Admin", IsActive = true },
                new ErpEmployee { Id = 2, Username = "manager", FullName = "Factory Manager", Role = "Manager", IsActive = true },
                new ErpEmployee { Id = 3, Username = "sales", FullName = "Sales User", Role = "Sales", IsActive = true },
                new ErpEmployee { Id = 4, Username = "warehouse", FullName = "Warehouse User", Role = "Warehouse", IsActive = true },
                new ErpEmployee { Id = 5, Username = "accountant", FullName = "Accountant User", Role = "Accountant", IsActive = true },
                new ErpEmployee { Id = 6, Username = "production", FullName = "Production User", Role = "Production", IsActive = true },
                new ErpEmployee { Id = 7, Username = "inactive", FullName = "Inactive User", Role = "Warehouse", IsActive = false }
            };

            foreach (var employee in employees)
            {
                employee.PasswordHash = PasswordHasher.HashPassword(employee, "Admin@123");
            }

            dbContext.Employees.AddRange(employees);
        }

        if (!await dbContext.Customers.AnyAsync(cancellationToken))
        {
            dbContext.Customers.AddRange(
                new ErpCustomer { Id = 1001, Code = "CUS001001", Name = "Công ty Minh Tâm", Phone = "0909123456", Email = "contact@minhtam.vn", Address = "Q1, TP.HCM", TaxCode = "0312345678", CurrentDebt = 12_500_000 },
                new ErpCustomer { Id = 1002, Code = "CUS001002", Name = "Shop Nắng Mai", Phone = "0988123456", Email = "hello@nangmai.vn", Address = "Bình Thạnh, TP.HCM", TaxCode = "0311112222", CurrentDebt = 0 },
                new ErpCustomer { Id = 1003, Code = "CUS001003", Name = "Anh Lê Hùng", Phone = "0911122233", Email = "", Address = "Thủ Đức, TP.HCM", TaxCode = "", CurrentDebt = 3_400_000 }
            );
        }

        if (!await dbContext.ProductCategories.AnyAsync(cancellationToken))
        {
            dbContext.ProductCategories.AddRange(
                new ErpProductCategory { Id = 1, Name = "In bạt", DefaultUnit = "M2", UseAreaPricing = true },
                new ErpProductCategory { Id = 2, Name = "In decal", DefaultUnit = "M2", UseAreaPricing = true },
                new ErpProductCategory { Id = 3, Name = "Bảng alu", DefaultUnit = "TẤM", UseAreaPricing = false },
                new ErpProductCategory { Id = 4, Name = "Mica", DefaultUnit = "TẤM", UseAreaPricing = false },
                new ErpProductCategory { Id = 5, Name = "Name card", DefaultUnit = "HỘP", UseAreaPricing = false },
                new ErpProductCategory { Id = 6, Name = "Tờ rơi", DefaultUnit = "XẤP", UseAreaPricing = false }
            );
        }

        if (!await dbContext.Orders.AnyAsync(cancellationToken))
        {
            var seed = InMemorySeed.Order;
            dbContext.Orders.Add(new ErpOrder { Id = 2000, OrderCode = seed.OrderCode, Payload = OrderPayloadMapper.Serialize(seed) });
        }

        if (!await dbContext.DashboardMaterials.AnyAsync(cancellationToken))
        {
            dbContext.DashboardMaterials.AddRange(
                new ErpDashboardMaterial { Id = 101, MaterialCode = "MAT-001", MaterialName = "Giấy Couche 150gsm", GroupName = "Giấy", CurrentStock = 250, MinStockLevel = 300, Unit = "tờ" },
                new ErpDashboardMaterial { Id = 102, MaterialCode = "MAT-002", MaterialName = "Mực Cyan", GroupName = "Mực in", CurrentStock = 12, MinStockLevel = 25, Unit = "chai" },
                new ErpDashboardMaterial { Id = 103, MaterialCode = "MAT-003", MaterialName = "Mực Magenta", GroupName = "Mực in", CurrentStock = 28, MinStockLevel = 25, Unit = "chai" },
                new ErpDashboardMaterial { Id = 104, MaterialCode = "MAT-004", MaterialName = "Keo dán", GroupName = "Phụ liệu", CurrentStock = 16, MinStockLevel = 20, Unit = "kg" },
                new ErpDashboardMaterial { Id = 105, MaterialCode = "MAT-005", MaterialName = "Màng cán bóng", GroupName = "Phụ liệu", CurrentStock = 45, MinStockLevel = 60, Unit = "cuộn" }
            );
        }

        if (!await dbContext.DashboardPayments.AnyAsync(cancellationToken))
        {
            dbContext.DashboardPayments.AddRange(
                new ErpDashboardPayment { PaymentDate = DateTime.UtcNow.AddDays(-1), OrderCode = "ORD-0004", CustomerName = "Hoa Sen Group", Amount = 20_000_000, Method = "Chuyển khoản" },
                new ErpDashboardPayment { PaymentDate = DateTime.UtcNow.AddDays(-2), OrderCode = "ORD-0007", CustomerName = "Long Châu", Amount = 15_000_000, Method = "Tiền mặt" },
                new ErpDashboardPayment { PaymentDate = DateTime.UtcNow.AddDays(-4), OrderCode = "ORD-0002", CustomerName = "Minh Long Foods", Amount = 10_000_000, Method = "Chuyển khoản" },
                new ErpDashboardPayment { PaymentDate = DateTime.UtcNow.AddDays(-7), OrderCode = "ORD-0006", CustomerName = "Fresh Mart", Amount = 8_000_000, Method = "Công nợ" },
                new ErpDashboardPayment { PaymentDate = DateTime.UtcNow.AddDays(-10), OrderCode = "ORD-0007", CustomerName = "Long Châu", Amount = 13_300_000, Method = "Chuyển khoản" }
            );
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

internal static class InMemorySeed
{
    public static OrderDetailViewModel Order => new()
    {
        Id = 2000,
        OrderCode = "ORD20262000",
        OrderDate = DateTime.Today.AddDays(-7),
        DeliveryDate = DateTime.Today.AddDays(2),
        Status = "PRODUCING",
        PaymentStatus = "PARTIAL",
        CustomerId = 1001,
        CustomerName = "Công ty Minh Tâm",
        CustomerPhone = "0909123456",
        CustomerEmail = "contact@minhtam.vn",
        CustomerAddress = "Q1, TP.HCM",
        SalesEmployeeName = "Sales User",
        AssignedEmployeeName = "Factory Manager",
        SubtotalAmount = 15_000_000,
        DiscountAmount = 500_000,
        TaxAmount = 1_450_000,
        TotalAmount = 15_950_000,
        PaidAmount = 6_000_000,
        RemainingAmount = 9_950_000,
        Note = "Ưu tiên giao trước khu vực lễ tân.",
        Items = [ new OrderDetailItemViewModel { Id = 1, ProductCategoryName = "In bạt", ItemName = "Backdrop khai trương", Description = "In bạt hiflex, bo viền", Width = 3, Height = 2, Unit = "M2", Quantity = 2, Area = 12, MaterialDescription = "Hiflex 3M", PrintType = "UV", FinishingDescription = "Bo viền + khoen", EstimatedUnitPrice = 450_000, EstimatedLineTotal = 5_400_000, EstimatedCost = 3_100_000, EstimatedProfit = 2_300_000, UnitPrice = 460_000, LineTotal = 5_520_000, PricingNote = "Override theo yêu cầu in dày", Note = "Thi công ban đêm" } ],
        Payments = [ new OrderPaymentViewModel { Id = 1, PaymentDate = DateTime.Today.AddDays(-5), Amount = 4_000_000, Method = "BANK", ReferenceCode = "VCB-001991", Note = "Đặt cọc lần 1", CreatedBy = "Accountant User" } ],
        StockOuts = [ new OrderStockOutViewModel { Id = 7001, Code = "SO-7001", StockOutDate = DateTime.Today.AddDays(-2), Purpose = "Xuất sản xuất đơn ORD20262000", CreatedBy = "Warehouse User", TotalAmount = 4_738_000 } ],
        StatusHistories = [ new OrderStatusHistoryViewModel { ChangedAt = DateTime.Today.AddDays(-7).AddHours(8), OldStatus = null, NewStatus = "NEW", ChangedBy = "Sales User", Note = "Tạo đơn" } ]
    };
}
