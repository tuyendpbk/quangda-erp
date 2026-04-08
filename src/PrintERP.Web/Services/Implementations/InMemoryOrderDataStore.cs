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
        new() { Id = 4, Name = "Warehouse User" },
        new() { Id = 5, Name = "Accountant User" },
        new() { Id = 6, Name = "Production User" }
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

    public static List<OrderDetailViewModel> Orders { get; } = BuildSeedOrders();

    public static string NextOrderCode()
    {
        _orderSeq++;
        return $"ORD{DateTime.UtcNow:yyyy}{_orderSeq:000}";
    }

    public static long NextOrderId() => _orderSeq;

    public static void AddOrder(OrderDetailViewModel order)
    {
        Orders.Add(order);
    }

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

    private static List<OrderDetailViewModel> BuildSeedOrders()
    {
        return
        [
            new OrderDetailViewModel
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
                Items =
                [
                    new OrderDetailItemViewModel
                    {
                        Id = 1,
                        ProductCategoryName = "In bạt",
                        ItemName = "Backdrop khai trương",
                        Description = "In bạt hiflex, bo viền",
                        Width = 3,
                        Height = 2,
                        Unit = "M2",
                        Quantity = 2,
                        Area = 12,
                        MaterialDescription = "Hiflex 3M",
                        PrintType = "UV",
                        FinishingDescription = "Bo viền + khoen",
                        EstimatedUnitPrice = 450_000,
                        EstimatedLineTotal = 5_400_000,
                        EstimatedCost = 3_100_000,
                        EstimatedProfit = 2_300_000,
                        UnitPrice = 460_000,
                        LineTotal = 5_520_000,
                        PricingNote = "Override theo yêu cầu in dày",
                        Note = "Thi công ban đêm"
                    },
                    new OrderDetailItemViewModel
                    {
                        Id = 2,
                        ProductCategoryName = "Bảng alu",
                        ItemName = "Bảng hiệu mặt tiền",
                        Description = "Khung sắt hộp + alu",
                        Width = 4,
                        Height = 1.2m,
                        Unit = "TẤM",
                        Quantity = 1,
                        Area = 4.8m,
                        MaterialDescription = "Alu ngoài trời",
                        PrintType = "Decal cán",
                        FinishingDescription = "Gia cố khung",
                        EstimatedUnitPrice = 8_000_000,
                        EstimatedLineTotal = 8_000_000,
                        EstimatedCost = 5_900_000,
                        EstimatedProfit = 2_100_000,
                        UnitPrice = 8_000_000,
                        LineTotal = 8_000_000,
                        Note = "Có bảo hành 12 tháng"
                    }
                ],
                Payments =
                [
                    new OrderPaymentViewModel
                    {
                        Id = 1,
                        PaymentDate = DateTime.Today.AddDays(-5),
                        Amount = 4_000_000,
                        Method = "BANK",
                        ReferenceCode = "VCB-001991",
                        Note = "Đặt cọc lần 1",
                        CreatedBy = "Accountant User"
                    },
                    new OrderPaymentViewModel
                    {
                        Id = 2,
                        PaymentDate = DateTime.Today.AddDays(-3),
                        Amount = 2_000_000,
                        Method = "CASH",
                        ReferenceCode = "PT000231",
                        Note = "Thu bổ sung",
                        CreatedBy = "Accountant User"
                    }
                ],
                MaterialUsages =
                [
                    new OrderMaterialUsageViewModel
                    {
                        OrderItemName = "Backdrop khai trương",
                        MaterialId = 10,
                        MaterialName = "Bạt hiflex",
                        PlannedQuantity = 12,
                        ActualQuantity = 12.4m,
                        UnitCost = 120_000,
                        TotalCost = 1_488_000,
                        Note = "Hao hụt mép"
                    },
                    new OrderMaterialUsageViewModel
                    {
                        OrderItemName = "Bảng hiệu mặt tiền",
                        MaterialId = 22,
                        MaterialName = "Alu ngoài trời",
                        PlannedQuantity = 5,
                        ActualQuantity = 5,
                        UnitCost = 650_000,
                        TotalCost = 3_250_000
                    }
                ],
                StockOuts =
                [
                    new OrderStockOutViewModel
                    {
                        Id = 7001,
                        Code = "SO-7001",
                        StockOutDate = DateTime.Today.AddDays(-2),
                        Purpose = "Xuất sản xuất đơn ORD20262000",
                        CreatedBy = "Warehouse User",
                        TotalAmount = 4_738_000
                    }
                ],
                StatusHistories =
                [
                    new OrderStatusHistoryViewModel
                    {
                        ChangedAt = DateTime.Today.AddDays(-7).AddHours(8),
                        OldStatus = null,
                        NewStatus = "NEW",
                        ChangedBy = "Sales User",
                        Note = "Tạo đơn"
                    },
                    new OrderStatusHistoryViewModel
                    {
                        ChangedAt = DateTime.Today.AddDays(-6).AddHours(9),
                        OldStatus = "NEW",
                        NewStatus = "DESIGN",
                        ChangedBy = "Factory Manager"
                    },
                    new OrderStatusHistoryViewModel
                    {
                        ChangedAt = DateTime.Today.AddDays(-4).AddHours(15),
                        OldStatus = "DESIGN",
                        NewStatus = "PRODUCING",
                        ChangedBy = "Production User",
                        Note = "Đã duyệt mockup"
                    }
                ]
            }
        ];
    }
}
