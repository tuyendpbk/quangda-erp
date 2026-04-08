using System.ComponentModel.DataAnnotations;

namespace PrintERP.Web.Models.ViewModels;

public class EmployeeCreateViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Vui lòng nhập mã nhân viên")]
    [MaxLength(50)]
    public string EmployeeCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [RegularExpression(@"^(0|\+84)\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? Department { get; set; }

    [MaxLength(100)]
    public string? Position { get; set; }

    [DataType(DataType.Date)]
    public DateTime? HireDate { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn vai trò")]
    public long? RoleId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
    [MaxLength(80)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "Mật khẩu không đáp ứng yêu cầu")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn trạng thái")]
    public string Status { get; set; } = "ACTIVE";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(Username) && Username != Username.Trim())
        {
            yield return new ValidationResult("Tên đăng nhập không được có khoảng trắng đầu/cuối.", [nameof(Username)]);
        }

        if (!string.IsNullOrWhiteSpace(Username) && Username.Any(char.IsWhiteSpace))
        {
            yield return new ValidationResult("Tên đăng nhập không hợp lệ.", [nameof(Username)]);
        }

        if (!string.Equals(Password, ConfirmPassword, StringComparison.Ordinal))
        {
            yield return new ValidationResult("Xác nhận mật khẩu không khớp", [nameof(ConfirmPassword)]);
        }

        if (HireDate.HasValue && HireDate.Value.Date > DateTime.Today)
        {
            yield return new ValidationResult("Ngày vào làm không hợp lệ", [nameof(HireDate)]);
        }

        if (!string.Equals(Status, "ACTIVE", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(Status, "INACTIVE", StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult("Vui lòng chọn trạng thái", [nameof(Status)]);
        }
    }
}

public class EmployeeCreatePageViewModel
{
    public EmployeeCreateViewModel Employee { get; set; } = new();
    public List<(long Id, string Code)> Roles { get; set; } = [];
    public List<string> Statuses { get; set; } = ["ACTIVE", "INACTIVE"];
    public List<string> Departments { get; set; } = ["Sales", "Warehouse", "Production", "Finance", "IT", "Management"];
}

public class EmployeeCreateResult
{
    public bool Success { get; set; }
    public long EmployeeId { get; set; }
    public Dictionary<string, string> Errors { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
