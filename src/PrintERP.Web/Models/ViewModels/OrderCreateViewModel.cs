using System.ComponentModel.DataAnnotations;

namespace PrintERP.Web.Models.ViewModels;

public static class OrderPermissions
{
    public const string Create = "ORDER_CREATE";
}

public class OrderCreateViewModel : IValidatableObject
{
    public string OrderCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn ngày tạo đơn")]
    [DataType(DataType.Date)]
    public DateTime OrderDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    public DateTime? DeliveryDate { get; set; }

    [Range(1, long.MaxValue, ErrorMessage = "Vui lòng chọn khách hàng")]
    public long CustomerId { get; set; }

    [Range(1, long.MaxValue, ErrorMessage = "Vui lòng chọn nhân viên sales")]
    public long SalesEmployeeId { get; set; }

    public long? AssignedEmployeeId { get; set; }

    [MaxLength(1000)]
    public string? Note { get; set; }

    public decimal SubtotalAmount { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Giảm giá không hợp lệ")]
    public decimal DiscountAmount { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Thuế không hợp lệ")]
    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    [MinLength(1, ErrorMessage = "Đơn hàng phải có ít nhất 1 hạng mục")]
    public List<OrderItemCreateViewModel> Items { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DeliveryDate.HasValue && DeliveryDate.Value.Date < OrderDate.Date)
        {
            yield return new ValidationResult("Ngày giao không được nhỏ hơn ngày tạo đơn.", [nameof(DeliveryDate)]);
        }

        if (DiscountAmount > SubtotalAmount)
        {
            yield return new ValidationResult("Giảm giá không hợp lệ", [nameof(DiscountAmount)]);
        }
    }
}

public class OrderItemCreateViewModel : IValidatableObject
{
    public long? ProductCategoryId { get; set; }

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

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!ProductCategoryId.HasValue || ProductCategoryId <= 0)
        {
            yield return new ValidationResult("Vui lòng chọn loại sản phẩm", [nameof(ProductCategoryId)]);
        }

        if (string.IsNullOrWhiteSpace(ItemName))
        {
            yield return new ValidationResult("Vui lòng nhập tên hạng mục", [nameof(ItemName)]);
        }

        if (Quantity <= 0)
        {
            yield return new ValidationResult("Số lượng phải lớn hơn 0", [nameof(Quantity)]);
        }

        if (UnitPrice < 0)
        {
            yield return new ValidationResult("Đơn giá không hợp lệ", [nameof(UnitPrice)]);
        }

        if (Width.HasValue && Width <= 0)
        {
            yield return new ValidationResult("Rộng phải lớn hơn 0", [nameof(Width)]);
        }

        if (Height.HasValue && Height <= 0)
        {
            yield return new ValidationResult("Cao phải lớn hơn 0", [nameof(Height)]);
        }
    }
}

public class OrderItemEstimateRequest
{
    public long? ProductCategoryId { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public decimal Quantity { get; set; }
    public string? Unit { get; set; }
    public string? MaterialDescription { get; set; }
    public string? PrintType { get; set; }
    public string? FinishingDescription { get; set; }
    public long? CustomerId { get; set; }
}

public class CustomerQuickCreateViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên khách hàng")]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [MaxLength(255)]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(50)]
    public string? TaxCode { get; set; }
}

public class CustomerSummaryViewModel
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxCode { get; set; }
    public decimal CurrentDebt { get; set; }
}

public class EmployeeOptionViewModel
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ProductCategoryOptionViewModel
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DefaultUnit { get; set; } = "CÁI";
    public bool UseAreaPricing { get; set; }
}

public class OrderItemEstimateResultViewModel
{
    public decimal Area { get; set; }
    public decimal EstimatedUnitPrice { get; set; }
    public decimal EstimatedLineTotal { get; set; }
    public decimal EstimatedCost { get; set; }
    public decimal EstimatedProfit { get; set; }
    public string PricingNote { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public class OrderCreatePageViewModel
{
    public OrderCreateViewModel Order { get; set; } = new();
    public List<CustomerSummaryViewModel> Customers { get; set; } = new();
    public List<EmployeeOptionViewModel> Employees { get; set; } = new();
    public List<ProductCategoryOptionViewModel> ProductCategories { get; set; } = new();
}
