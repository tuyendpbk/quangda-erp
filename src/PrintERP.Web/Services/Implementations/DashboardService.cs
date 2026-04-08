using Microsoft.EntityFrameworkCore;
using PrintERP.Web.Data;
using PrintERP.Web.Models.ViewModels;
using PrintERP.Web.Services.Interfaces;

namespace PrintERP.Web.Services.Implementations;

public class DashboardService(AppDbContext dbContext) : IDashboardService
{
    public async Task<DashboardViewModel> GetDashboardAsync(string role, DashboardFilterViewModel filter, CancellationToken cancellationToken = default)
    {
        var normalizedFilter = NormalizeFilter(filter);
        var range = ResolveRange(normalizedFilter);

        var payloads = await dbContext.Orders.AsNoTracking().Select(x => x.Payload).ToListAsync(cancellationToken);
        var orders = payloads.Select(OrderPayloadMapper.Deserialize).Where(x => x is not null).Select(x => new OrderRecord(x!.Id, x.OrderCode, x.CustomerName, x.OrderDate, x.DeliveryDate ?? x.OrderDate, x.Status, x.TotalAmount, x.SalesEmployeeName ?? "-")).ToList();
        var payments = await dbContext.DashboardPayments.AsNoTracking().ToListAsync(cancellationToken);
        var materials = await dbContext.DashboardMaterials.AsNoTracking().ToListAsync(cancellationToken);

        var ordersInRange = orders
            .Where(x => x.OrderDate.Date >= range.from.Date && x.OrderDate.Date <= range.to.Date)
            .ToList();

        var paidAmount = payments
            .Where(x => x.PaymentDate.Date >= range.from.Date && x.PaymentDate.Date <= range.to.Date)
            .Sum(x => x.Amount);

        var revenue = ordersInRange
            .Where(x => x.Status == "DELIVERED")
            .Sum(x => x.TotalAmount);

        var dueOrders = ordersInRange
            .Where(x => x.Status != "DELIVERED" && x.Status != "CANCELLED")
            .Select(x => new DashboardOrderItemViewModel
            {
                Id = (int)x.Id,
                OrderCode = x.OrderCode,
                CustomerName = x.CustomerName,
                DeliveryDate = x.DeliveryDate,
                Status = x.Status,
                OrderDate = x.OrderDate,
                TotalAmount = x.TotalAmount,
                Owner = x.Owner,
                WarningLevel = x.DeliveryDate.Date < DateTime.UtcNow.Date ? "OVERDUE" : (x.DeliveryDate.Date <= DateTime.UtcNow.Date.AddDays(3) ? "NEAR_DUE" : "NORMAL")
            })
            .OrderBy(x => x.DeliveryDate)
            .Take(10)
            .ToList();

        var topCustomers = ordersInRange
            .GroupBy(x => x.CustomerName)
            .Select(g =>
            {
                var amount = g.Sum(x => x.TotalAmount);
                var paid = payments.Where(x => x.CustomerName == g.Key).Sum(x => x.Amount);
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

        var lowStockItems = materials
            .Where(x => x.CurrentStock <= x.MinStockLevel)
            .OrderBy(x => x.CurrentStock - x.MinStockLevel)
            .Select(x => new DashboardLowStockItemViewModel
            {
                Id = (int)x.Id,
                MaterialCode = x.MaterialCode,
                MaterialName = x.MaterialName,
                GroupName = x.GroupName,
                CurrentStock = x.CurrentStock,
                MinStockLevel = x.MinStockLevel,
                Unit = x.Unit
            })
            .ToList();

        return new DashboardViewModel
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
            LowStockMaterials = lowStockItems.Count(),
            RevenueChart = BuildRevenueChart(ordersInRange, range.from, range.to),
            OrderStatusChart = BuildOrderStatusChart(ordersInRange),
            PaymentChart = BuildPaymentChart(ordersInRange, paidAmount),
            RecentOrders = ordersInRange.OrderByDescending(x => x.OrderDate).Take(10).Select(x => new DashboardOrderItemViewModel
            {
                Id = (int)x.Id,
                OrderCode = x.OrderCode,
                OrderDate = x.OrderDate,
                CustomerName = x.CustomerName,
                DeliveryDate = x.DeliveryDate,
                Status = x.Status,
                TotalAmount = x.TotalAmount,
                Owner = x.Owner
            }).ToList(),
            DueOrders = dueOrders,
            LowStockItems = lowStockItems,
            RecentPayments = payments.Where(x => x.PaymentDate.Date >= range.from.Date && x.PaymentDate.Date <= range.to.Date)
                .OrderByDescending(x => x.PaymentDate)
                .Take(10)
                .Select(x => new DashboardPaymentItemViewModel
                {
                    PaymentDate = x.PaymentDate,
                    OrderCode = x.OrderCode,
                    CustomerName = x.CustomerName,
                    Amount = x.Amount,
                    Method = x.Method
                }).ToList(),
            TopCustomers = topCustomers,
            ForecastRevenue = revenue * 1.12m,
            ShowPaymentBlocks = DashboardRoles.PaymentRoles.Contains(role),
            ShowForecastBlock = DashboardRoles.ForecastRoles.Contains(role),
            ShowTopCustomers = DashboardRoles.TopCustomerRoles.Contains(role)
        };
    }

    private static DashboardFilterViewModel NormalizeFilter(DashboardFilterViewModel? filter)
    {
        var periodType = filter?.PeriodType?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(periodType) || !DashboardPeriodTypes.Supported.Contains(periodType)) periodType = DashboardPeriodTypes.ThisMonth;
        var normalized = new DashboardFilterViewModel { PeriodType = periodType, FromDate = filter?.FromDate?.Date, ToDate = filter?.ToDate?.Date };
        if (normalized.PeriodType == DashboardPeriodTypes.Custom && normalized.FromDate.HasValue && normalized.ToDate.HasValue && normalized.FromDate > normalized.ToDate)
            (normalized.FromDate, normalized.ToDate) = (normalized.ToDate, normalized.FromDate);
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
            DashboardPeriodTypes.Custom when filter.FromDate.HasValue && filter.ToDate.HasValue => (filter.FromDate.Value.Date, filter.ToDate.Value.Date),
            _ => (new DateTime(today.Year, today.Month, 1), today)
        };
    }

    private static List<RevenueChartItemViewModel> BuildRevenueChart(IEnumerable<OrderRecord> orders, DateTime from, DateTime to)
    {
        var days = (to - from).TotalDays;
        if (days <= 31)
        {
            return orders.Where(x => x.Status == "DELIVERED").GroupBy(x => x.OrderDate.Date).OrderBy(x => x.Key)
                .Select(x => new RevenueChartItemViewModel { Label = x.Key.ToString("dd/MM"), Revenue = x.Sum(y => y.TotalAmount) }).ToList();
        }

        return orders.Where(x => x.Status == "DELIVERED").GroupBy(x => new DateTime(x.OrderDate.Year, x.OrderDate.Month, 1)).OrderBy(x => x.Key)
            .Select(x => new RevenueChartItemViewModel { Label = x.Key.ToString("MM/yyyy"), Revenue = x.Sum(y => y.TotalAmount) }).ToList();
    }

    private static List<OrderStatusChartItemViewModel> BuildOrderStatusChart(IEnumerable<OrderRecord> orders)
        => new[] { "NEW", "DESIGN", "PRODUCING", "DONE", "DELIVERED", "CANCELLED" }
            .Select(status => new OrderStatusChartItemViewModel { Status = status, Count = orders.Count(x => x.Status == status) }).ToList();

    private static List<PaymentChartItemViewModel> BuildPaymentChart(IEnumerable<OrderRecord> orders, decimal paidAmount)
    {
        var totalAmount = orders.Sum(x => x.TotalAmount);
        return
        [
            new PaymentChartItemViewModel
            {
                Label = "Công nợ",
                TotalOrderAmount = totalAmount,
                PaidAmount = Math.Min(paidAmount, totalAmount),
                RemainingDebt = Math.Max(0, totalAmount - paidAmount)
            }
        ];
    }

    private sealed record OrderRecord(long Id, string OrderCode, string CustomerName, DateTime OrderDate, DateTime DeliveryDate, string Status, decimal TotalAmount, string Owner);
}
