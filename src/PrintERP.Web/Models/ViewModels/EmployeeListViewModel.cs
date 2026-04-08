using System.ComponentModel.DataAnnotations;

namespace PrintERP.Web.Models.ViewModels;

public static class EmployeePermissions
{
    public const string View = "EMPLOYEE_VIEW";
    public const string Create = "EMPLOYEE_CREATE";
    public const string Edit = "EMPLOYEE_EDIT";
    public const string ResetPassword = "EMPLOYEE_RESET_PASSWORD";
}

public class EmployeeFilterViewModel : IValidatableObject
{
    public string? Keyword { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public long? RoleId { get; set; }
    public string? Status { get; set; }

    [DataType(DataType.Date)]
    public DateTime? HireDateFrom { get; set; }

    [DataType(DataType.Date)]
    public DateTime? HireDateTo { get; set; }

    public string? SortBy { get; set; } = "employeeCode";
    public string? SortDirection { get; set; } = "asc";

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (HireDateFrom.HasValue && HireDateTo.HasValue && HireDateFrom.Value.Date > HireDateTo.Value.Date)
        {
            yield return new ValidationResult("Khoảng ngày vào làm không hợp lệ", [nameof(HireDateFrom), nameof(HireDateTo)]);
        }
    }
}

public class EmployeeListItemViewModel
{
    public long Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }

    public string? Department { get; set; }
    public string? Position { get; set; }

    public long RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;
    public DateTime? HireDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public int AssignedOrderCount { get; set; }

    public bool CanView { get; set; }
    public bool CanEdit { get; set; }
    public bool CanToggleStatus { get; set; }
    public bool CanResetPassword { get; set; }
}

public class EmployeeListViewModel
{
    public EmployeeFilterViewModel Filter { get; set; } = new();
    public List<EmployeeListItemViewModel> Items { get; set; } = [];

    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int InactiveEmployees { get; set; }

    public List<string> Departments { get; set; } = [];
    public List<(long Id, string Name)> Roles { get; set; } = [];

    public bool CanCreateEmployee { get; set; }
    public bool CanExport { get; set; }
}
