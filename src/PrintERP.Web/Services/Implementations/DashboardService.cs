using PrintERP.Web.Models.ViewModels;
using PrintERP.Web.Services.Interfaces;

namespace PrintERP.Web.Services.Implementations;

public class DashboardService : IDashboardService
{
    private static readonly string[] ActiveStatuses = ["NEW", "DESIGN", "PRODUCING", "DONE"];

    private static readonly IReadOnlyList<OrderRecord> Orders =
    [
        new(1, "ORD-0001", "An Bình Printing", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), "NEW", 12_000_000m, "Hải"),
        new(2, "ORD-0002", "Minh Long Foods", DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(2), "PRODUCING", 22_500_000m, "Nam"),
        new(3, "ORD-0003", "VietShop", DateTime.UtcNow.AddDays(-6), DateTime.UtcNow.AddDays(-1), "PRODUCING", 8_500_000m, "Khánh"),
        new(4, "ORD-0004", "Hoa Sen Group", DateTime.UtcNow.AddDays(-8), DateTime.UtcNow.AddDays(-4), "DELIVERED", 36_000_000m, "Linh"),
        new(5, "ORD-0005", "Green Life", DateTime.UtcNow.AddDays(-12), DateTime.UtcNow.AddDays(3), "DESIGN", 15_500_000m, "Trang"),
        new(6, "ORD-0006", "Fresh Mart", DateTime.UtcNow.AddDays(-15), DateTime.UtcNow.AddDays(-3), "DONE", 17_250_000m, "Sơn"),
        new(7, "ORD-0007", "Long Châu", DateTime.UtcNow.AddDays(-20), DateTime.UtcNow.AddDays(-6), "DELIVERED", 28_300_000m, "Mai"),
        new(8, "ORD-0008", "Hưng Thịnh", DateTime.UtcNow.AddDays(-25), DateTime.UtcNow.AddDays(5), "CANCELLED", 7_000_000m, "Nam")
    ];

    private static readonly IReadOnlyList<MaterialRecord> Materials =
    [
        new(101, "MAT-001", "Giấy Couche 150gsm", "Giấy", 250, 300, "tờ"),
        new(102, "MAT-002", "Mực Cyan", "Mực in", 12, 25, "chai"),
        new(103, "MAT-003", "Mực Magenta", "Mực in", 28, 25, "chai"),
        new(104, "MAT-004", "Keo dán", "Phụ liệu", 16, 20, "kg"),
        new(105, "MAT-005", "Màng cán bóng", "Phụ liệu", 45, 60, "cuộn")
    ];

    private static readonly IReadOnlyList<PaymentRecord> Payments =
    [
        new(DateTime.UtcNow.AddDays(-1), "ORD-0004", "Hoa Sen Group", 20_000_000m, "Chuyển khoản"),
        new(DateTime.UtcNow.AddDays(-2), "ORD-0007", "Long Châu", 15_000_000m, "Tiền mặt"),
        new(DateTime.UtcNow.AddDays(-4), "ORD-0002", "Minh Long Foods", 10_000_000m, "Chuyển khoản"),
        new(DateTime.UtcNow.AddDays(-7), "ORD-0006", "Fresh Mart", 8_000_000m, "Công nợ"),
        new(DateTime.UtcNow.AddDays(-10), "ORD-0007", "Long Châu", 13_300_000m, "Chuyển khoản")
    ];

    public Task<DashboardViewModel> GetDashboardAsync(string role, DashboardFilterViewModel filter, CancellationToken cancellationToken = default)
    {
        var normalizedFilter = NormalizeFilter(filter);
        var range = ResolveRange(normalizedFilter);

        var ordersInRange = Orders
            .Where(x => x.OrderDate.Date >= range.from.Date && x.OrderDate.Date <= range.to.Date)
            .ToList();

        var paidAmount = Payments
            .Where(x => x.PaymentDate.Date >= range.from.Date && x.PaymentDate.Date <= range.to.Date)
            .Sum(x => x.Amount);

        var revenue = ordersInRange
            .Where(x => x.Status == "DELIVERED")
            .Sum(x => x.TotalAmount);

        var dueOrders = ordersInRange
            .Where(x => x.Status != "DELIVERED" && x.Status != "CANCELLED")
            .Select(x => new DashboardOrderItemViewModel
            {
                Id = x.Id,
                OrderCode = x.OrderCode,
                CustomerName = x.CustomerName,
                DeliveryDate = x.DeliveryDate,
                Status = x.Status,
                OrderDate = x.OrderDate,
                TotalAmount = x.TotalAmount,
                Owner = x.Owner,
                WarningLevel = x.DeliveryDate.Date < DateTime.UtcNow.Date
                    ? "OVERDUE"
                    : (x.DeliveryDate.Date <= DateTime.UtcNow.Date.AddDays(3) ? "NEAR_DUE" : "NORMAL")
            })
            .OrderBy(x => x.DeliveryDate)
            .Take(10)
            .ToList();

        var topCustomers = ordersInRange
            .GroupBy(x => x.CustomerName)
            .Select(g =>
            {
                var amount = g.Sum(x => x.TotalAmount);
                var paid = Payments.Where(x => x.CustomerName == g.Key).Sum(x => x.Amount);
                return new DashboardTopCustomerItemViewModel
                {
                    CustomerName = g.Key,
                    OrderCount = g.Count(),
                    Revenue = amount,
                    RemainingDebt = Math.Max(0, amount - paid)
                };
            })
            .OrderByDescending(x => x.Revenue)
            .Take(5)
            .ToList();

        var lowStockItems = Materials
            .Where(x => x.CurrentStock <= x.MinStockLevel)
            .OrderBy(x => x.CurrentStock - x.MinStockLevel)
            .Select(x => new DashboardLowStockItemViewModel
            {
                Id = x.Id,
                MaterialCode = x.MaterialCode,
                MaterialName = x.MaterialName,
                GroupName = x.GroupName,
                CurrentStock = x.CurrentStock,
                MinStockLevel = x.MinStockLevel,
                Unit = x.Unit
            })
            .ToList();

        var model = new DashboardViewModel
        {
            Filter = normalizedFilter,
            TotalOrders = ordersInRange.Count,
            NewOrders = ordersInRange.Count(x => x.Status == "NEW"),
            ProducingOrders = ordersInRange.Count(x => x.Status == "PRODUCING"),
            NearDueOrders = dueOrders.Count(x => x.WarningLevel == "NEAR_DUE"),
            OverdueOrders = dueOrders.Count(x => x.WarningLevel == "OVERDUE"),
            Revenue = revenue,
            PaidAmount = paidAmount,
            RemainingDebt = Math.Max(0, ordersInRange.Sum(x => x.TotalAmount) - paidAmount),
            LowStockMaterials = lowStockItems.Count,
            RevenueChart = BuildRevenueChart(ordersInRange, range.from, range.to),
            OrderStatusChart = BuildOrderStatusChart(ordersInRange),
            PaymentChart = BuildPaymentChart(ordersInRange, paidAmount),
            RecentOrders = ordersInRange
                .OrderByDescending(x => x.OrderDate)
                .Take(10)
                .Select(x => new DashboardOrderItemViewModel
                {
                    Id = x.Id,
                    OrderCode = x.OrderCode,
                    OrderDate = x.OrderDate,
                    CustomerName = x.CustomerName,
                    DeliveryDate = x.DeliveryDate,
                    Status = x.Status,
                    TotalAmount = x.TotalAmount,
                    Owner = x.Owner
                })
                .ToList(),
            DueOrders = dueOrders,
            LowStockItems = lowStockItems,
            RecentPayments = Payments
                .Where(x => x.PaymentDate.Date >= range.from.Date && x.PaymentDate.Date <= range.to.Date)
                .OrderByDescending(x => x.PaymentDate)
                .Take(10)
                .Select(x => new DashboardPaymentItemViewModel
                {
                    PaymentDate = x.PaymentDate,
                    OrderCode = x.OrderCode,
                    CustomerName = x.CustomerName,
                    Amount = x.Amount,
                    Method = x.Method
                })
                .ToList(),
            TopCustomers = topCustomers,
            ForecastRevenue = revenue * 1.12m,
            ShowPaymentBlocks = DashboardRoles.PaymentRoles.Contains(role),
            ShowForecastBlock = DashboardRoles.ForecastRoles.Contains(role),
            ShowTopCustomers = DashboardRoles.TopCustomerRoles.Contains(role)
        };

        return Task.FromResult(model);
    }

    private static DashboardFilterViewModel NormalizeFilter(DashboardFilterViewModel? filter)
    {
        var periodType = filter?.PeriodType?.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(periodType) || !DashboardPeriodTypes.Supported.Contains(periodType))
        {
            periodType = DashboardPeriodTypes.ThisMonth;
        }

        var normalized = new DashboardFilterViewModel
        {
            PeriodType = periodType,
            FromDate = filter?.FromDate?.Date,
            ToDate = filter?.ToDate?.Date
        };

        if (normalized.PeriodType == DashboardPeriodTypes.Custom &&
            normalized.FromDate.HasValue &&
            normalized.ToDate.HasValue &&
            normalized.FromDate > normalized.ToDate)
        {
            (normalized.FromDate, normalized.ToDate) = (normalized.ToDate, normalized.FromDate);
        }

        return normalized;
    }

    private static (DateTime from, DateTime to) ResolveRange(DashboardFilterViewModel filter)
    {
        var today = DateTime.UtcNow.Date;

        return filter.PeriodType switch
        {
            DashboardPeriodTypes.Today => (today, today),
            DashboardPeriodTypes.Last7Days => (today.AddDays(-6), today),
            DashboardPeriodTypes.Last30Days => (today.AddDays(-29), today),
            DashboardPeriodTypes.ThisQuarter => (new DateTime(today.Year, ((today.Month - 1) / 3 * 3) + 1, 1), today),
            DashboardPeriodTypes.ThisYear => (new DateTime(today.Year, 1, 1), today),
            DashboardPeriodTypes.Custom when filter.FromDate.HasValue && filter.ToDate.HasValue
                => (filter.FromDate.Value.Date, filter.ToDate.Value.Date),
            _ => (new DateTime(today.Year, today.Month, 1), today)
        };
    }

    private static List<RevenueChartItemViewModel> BuildRevenueChart(IEnumerable<OrderRecord> orders, DateTime from, DateTime to)
    {
        var days = (to - from).TotalDays;

        if (days <= 31)
        {
            return orders
                .Where(x => x.Status == "DELIVERED")
                .GroupBy(x => x.OrderDate.Date)
                .OrderBy(x => x.Key)
                .Select(x => new RevenueChartItemViewModel
                {
                    Label = x.Key.ToString("dd/MM"),
                    Revenue = x.Sum(y => y.TotalAmount)
                })
                .ToList();
        }

        return orders
            .Where(x => x.Status == "DELIVERED")
            .GroupBy(x => new DateTime(x.OrderDate.Year, x.OrderDate.Month, 1))
            .OrderBy(x => x.Key)
            .Select(x => new RevenueChartItemViewModel
            {
                Label = x.Key.ToString("MM/yyyy"),
                Revenue = x.Sum(y => y.TotalAmount)
            })
            .ToList();
    }

    private static List<OrderStatusChartItemViewModel> BuildOrderStatusChart(IEnumerable<OrderRecord> orders)
    {
        var allStatuses = new[] { "NEW", "DESIGN", "PRODUCING", "DONE", "DELIVERED", "CANCELLED" };

        return allStatuses
            .Select(status => new OrderStatusChartItemViewModel
            {
                Status = status,
                Count = orders.Count(x => x.Status == status)
            })
            .ToList();
    }

    private static List<PaymentChartItemViewModel> BuildPaymentChart(IEnumerable<OrderRecord> orders, decimal paidAmount)
    {
        var totalAmount = orders.Sum(x => x.TotalAmount);

        return
        [
            new PaymentChartItemViewModel
            {
                Label = "Thanh toán",
                TotalOrderAmount = totalAmount,
                PaidAmount = paidAmount,
                RemainingDebt = Math.Max(0, totalAmount - paidAmount)
            }
        ];
    }

    private sealed record OrderRecord(int Id, string OrderCode, string CustomerName, DateTime OrderDate, DateTime DeliveryDate, string Status, decimal TotalAmount, string Owner);
    private sealed record MaterialRecord(int Id, string MaterialCode, string MaterialName, string GroupName, decimal CurrentStock, decimal MinStockLevel, string Unit);
    private sealed record PaymentRecord(DateTime PaymentDate, string OrderCode, string CustomerName, decimal Amount, string Method);
}
