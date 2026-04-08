using System.ComponentModel.DataAnnotations;

namespace PrintERP.Web.Models.ViewModels;

public class OrderStatusUpdateViewModel : IValidatableObject
{
    public long OrderId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }
    public DateTime? DeliveryDate { get; set; }

    public string CurrentStatus { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn trạng thái mới")]
    public string NewStatus { get; set; } = string.Empty;

    public string? Note { get; set; }

    public List<string> AvailableStatuses { get; set; } = [];

    public bool HasPayments { get; set; }
    public bool HasStockOut { get; set; }
    public bool IsLocked { get; set; }
    public bool IsOverdue => DeliveryDate.HasValue
                             && DeliveryDate.Value.Date < DateTime.Today
                             && !string.Equals(CurrentStatus, "DELIVERED", StringComparison.OrdinalIgnoreCase)
                             && !string.Equals(CurrentStatus, "CANCELLED", StringComparison.OrdinalIgnoreCase);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.Equals(NewStatus, "CANCELLED", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(Note))
        {
            yield return new ValidationResult("Vui lòng nhập lý do hủy đơn", [nameof(Note)]);
        }
    }
}
