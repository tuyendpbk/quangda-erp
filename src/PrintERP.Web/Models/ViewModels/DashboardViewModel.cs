namespace PrintERP.Web.Models.ViewModels;

public static class DashboardRoles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Sales = "Sales";
    public const string Warehouse = "Warehouse";
    public const string Accountant = "Accountant";
    public const string Production = "Production";

    public static readonly HashSet<string> PaymentRoles =
        [Admin, Manager, Accountant];

    public static readonly HashSet<string> ForecastRoles =
        [Admin, Manager];

    public static readonly HashSet<string> TopCustomerRoles =
        [Admin, Manager, Sales];
}

public static class DashboardPermissions
{
    public const string View = "DASHBOARD_VIEW";
}

public static class DashboardPeriodTypes
{
    public const string Today = "TODAY";
    public const string Last7Days = "LAST7DAYS";
    public const string Last30Days = "LAST30DAYS";
    public const string ThisMonth = "THISMONTH";
    public const string ThisQuarter = "THISQUARTER";
    public const string ThisYear = "THISYEAR";
    public const string Custom = "CUSTOM";

    public static readonly IReadOnlyList<string> Supported =
        [Today, Last7Days, Last30Days, ThisMonth, ThisQuarter, ThisYear, Custom];
}

public class DashboardFilterViewModel
{
    public string? PeriodType { get; set; } = DashboardPeriodTypes.ThisMonth;

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }
}

public class DashboardViewModel
{
    public DashboardFilterViewModel Filter { get; set; } = new();

    public int TotalOrders { get; set; }
    public int NewOrders { get; set; }
    public int ProducingOrders { get; set; }
    public int NearDueOrders { get; set; }
    public int OverdueOrders { get; set; }

    public decimal Revenue { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingDebt { get; set; }

    public int LowStockMaterials { get; set; }

    public List<RevenueChartItemViewModel> RevenueChart { get; set; } = new();
    public List<OrderStatusChartItemViewModel> OrderStatusChart { get; set; } = new();
    public List<PaymentChartItemViewModel> PaymentChart { get; set; } = new();

    public List<DashboardOrderItemViewModel> RecentOrders { get; set; } = new();
    public List<DashboardOrderItemViewModel> DueOrders { get; set; } = new();
    public List<DashboardLowStockItemViewModel> LowStockItems { get; set; } = new();
    public List<DashboardPaymentItemViewModel> RecentPayments { get; set; } = new();
    public List<DashboardTopCustomerItemViewModel> TopCustomers { get; set; } = new();

    public decimal? ForecastRevenue { get; set; }

    public bool ShowPaymentBlocks { get; set; }
    public bool ShowForecastBlock { get; set; }
    public bool ShowTopCustomers { get; set; }
}

public class RevenueChartItemViewModel
{
    public required string Label { get; set; }
    public decimal Revenue { get; set; }
}

public class OrderStatusChartItemViewModel
{
    public required string Status { get; set; }
    public int Count { get; set; }
}

public class PaymentChartItemViewModel
{
    public required string Label { get; set; }
    public decimal TotalOrderAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingDebt { get; set; }
}

public class DashboardOrderItemViewModel
{
    public int Id { get; set; }
    public required string OrderCode { get; set; }
    public DateTime OrderDate { get; set; }
    public required string CustomerName { get; set; }
    public DateTime DeliveryDate { get; set; }
    public required string Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Owner { get; set; }
    public string WarningLevel { get; set; } = "NORMAL";
}

public class DashboardLowStockItemViewModel
{
    public int Id { get; set; }
    public required string MaterialCode { get; set; }
    public required string MaterialName { get; set; }
    public required string GroupName { get; set; }
    public decimal CurrentStock { get; set; }
    public decimal MinStockLevel { get; set; }
    public required string Unit { get; set; }
}

public class DashboardPaymentItemViewModel
{
    public DateTime PaymentDate { get; set; }
    public required string OrderCode { get; set; }
    public required string CustomerName { get; set; }
    public decimal Amount { get; set; }
    public required string Method { get; set; }
}

public class DashboardTopCustomerItemViewModel
{
    public required string CustomerName { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal RemainingDebt { get; set; }
}
