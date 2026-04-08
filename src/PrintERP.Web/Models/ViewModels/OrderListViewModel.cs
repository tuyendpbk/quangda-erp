using System.ComponentModel.DataAnnotations;

namespace PrintERP.Web.Models.ViewModels;

public class OrderFilterViewModel : IValidatableObject
{
    public string? Keyword { get; set; }
    public string? Status { get; set; }
    public string? PaymentStatus { get; set; }

    public long? CustomerId { get; set; }
    public long? SalesEmployeeId { get; set; }
    public long? AssignedEmployeeId { get; set; }

    [DataType(DataType.Date)]
    public DateTime? OrderDateFrom { get; set; }

    [DataType(DataType.Date)]
    public DateTime? OrderDateTo { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DeliveryDateFrom { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DeliveryDateTo { get; set; }

    public bool IsOverdueOnly { get; set; }
    public bool IsNearDueOnly { get; set; }

    public string? SortBy { get; set; } = "orderDate";
    public string? SortDirection { get; set; } = "desc";

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (OrderDateFrom.HasValue && OrderDateTo.HasValue && OrderDateFrom.Value.Date > OrderDateTo.Value.Date)
        {
            yield return new ValidationResult("Khoảng ngày tạo không hợp lệ", [nameof(OrderDateFrom), nameof(OrderDateTo)]);
        }

        if (DeliveryDateFrom.HasValue && DeliveryDateTo.HasValue && DeliveryDateFrom.Value.Date > DeliveryDateTo.Value.Date)
        {
            yield return new ValidationResult("Khoảng ngày giao không hợp lệ", [nameof(DeliveryDateFrom), nameof(DeliveryDateTo)]);
        }
    }
}

public class OrderListItemViewModel
{
    public long Id { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? DeliveryDate { get; set; }

    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }

    public string SalesEmployeeName { get; set; } = string.Empty;
    public string? AssignedEmployeeName { get; set; }

    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;

    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }

    public int ItemCount { get; set; }
    public string? Note { get; set; }

    public bool IsOverdue { get; set; }
    public bool IsNearDue { get; set; }

    public bool CanView { get; set; }
    public bool CanEdit { get; set; }
    public bool CanUpdateStatus { get; set; }
    public bool CanRecordPayment { get; set; }
    public bool CanCreateStockOut { get; set; }
    public bool CanExportPdf { get; set; }
}

public class OrderListViewModel
{
    public OrderFilterViewModel Filter { get; set; } = new();
    public List<OrderListItemViewModel> Items { get; set; } = [];

    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    public decimal TotalOrderAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal TotalRemainingAmount { get; set; }

    public List<CustomerSummaryViewModel> Customers { get; set; } = [];
    public List<EmployeeOptionViewModel> Employees { get; set; } = [];
    public bool CanCreateOrder { get; set; }
    public bool CanExport { get; set; }
}
