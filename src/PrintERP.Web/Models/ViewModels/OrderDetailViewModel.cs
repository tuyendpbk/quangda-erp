using System.ComponentModel.DataAnnotations;

namespace PrintERP.Web.Models.ViewModels;

public static class OrderPermissions
{
    public const string Create = "ORDER_CREATE";
    public const string View = "ORDER_VIEW";
    public const string Edit = "ORDER_EDIT";
    public const string StatusUpdate = "ORDER_STATUS_UPDATE";
}

public static class PaymentPermissions
{
    public const string Create = "PAYMENT_CREATE";
}

public static class StockOutPermissions
{
    public const string Create = "STOCKOUT_CREATE";
}

public class OrderDetailViewModel
{
    public long Id { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? DeliveryDate { get; set; }

    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;

    public long CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    [EmailAddress]
    public string? CustomerEmail { get; set; }
    public string? CustomerAddress { get; set; }

    public string SalesEmployeeName { get; set; } = string.Empty;
    public string? AssignedEmployeeName { get; set; }

    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }

    public string? Note { get; set; }

    public List<OrderDetailItemViewModel> Items { get; set; } = [];
    public List<OrderPaymentViewModel> Payments { get; set; } = [];
    public List<OrderMaterialUsageViewModel> MaterialUsages { get; set; } = [];
    public List<OrderStockOutViewModel> StockOuts { get; set; } = [];
    public List<OrderStatusHistoryViewModel> StatusHistories { get; set; } = [];

    public bool CanEdit { get; set; }
    public bool CanUpdateStatus { get; set; }
    public bool CanRecordPayment { get; set; }
    public bool CanCreateStockOut { get; set; }
    public bool CanExportPdf { get; set; }

    public bool IsOverdue => DeliveryDate.HasValue
                             && DeliveryDate.Value.Date < DateTime.Today
                             && !string.Equals(Status, "DELIVERED", StringComparison.OrdinalIgnoreCase);
}

public class OrderDetailItemViewModel
{
    public long Id { get; set; }
    public string? ProductCategoryName { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }

    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public string? Unit { get; set; }
    public decimal Quantity { get; set; }
    public decimal Area { get; set; }

    public string? MaterialDescription { get; set; }
    public string? PrintType { get; set; }
    public string? FinishingDescription { get; set; }

    public decimal EstimatedUnitPrice { get; set; }
    public decimal EstimatedLineTotal { get; set; }
    public decimal EstimatedCost { get; set; }
    public decimal EstimatedProfit { get; set; }

    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    public string? PricingNote { get; set; }
    public string? Note { get; set; }
}

public class OrderPaymentViewModel
{
    public long Id { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string? ReferenceCode { get; set; }
    public string? Note { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class OrderStatusHistoryViewModel
{
    public DateTime ChangedAt { get; set; }
    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty;
    public string? Note { get; set; }
}

public class OrderMaterialUsageViewModel
{
    public string OrderItemName { get; set; } = string.Empty;
    public long? MaterialId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public decimal PlannedQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string? Note { get; set; }
}

public class OrderStockOutViewModel
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime StockOutDate { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}
