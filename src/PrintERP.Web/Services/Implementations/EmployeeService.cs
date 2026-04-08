using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PrintERP.Web.Data;
using PrintERP.Web.Data.Entities;
using PrintERP.Web.Models.ViewModels;
using PrintERP.Web.Services.Interfaces;

namespace PrintERP.Web.Services.Implementations;

public class EmployeeService(AppDbContext dbContext) : IEmployeeService
{
    private static readonly PasswordHasher<ErpEmployee> PasswordHasher = new();
    private static readonly (long Id, string Code, string Name, string DisplayName)[] Roles =
    [
        (1, "ADMIN", DashboardRoles.Admin, "Administrator"),
        (2, "MANAGER", DashboardRoles.Manager, "Manager"),
        (3, "SALES", DashboardRoles.Sales, "Sales"),
        (4, "WAREHOUSE", DashboardRoles.Warehouse, "Warehouse"),
        (5, "ACCOUNTANT", DashboardRoles.Accountant, "Accountant"),
        (6, "PRODUCTION", DashboardRoles.Production, "Production")
    ];

    public async Task<EmployeeListViewModel> GetListAsync(EmployeeFilterViewModel filter, ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        var normalizedFilter = NormalizeFilter(filter);
        var hasPermission = (string permission) => user.Claims.Any(x => x.Type == "Permission" && x.Value == permission);

        var employees = await dbContext.Employees.AsNoTracking().ToListAsync(cancellationToken);
        var assignedOrderCountMap = await BuildAssignedOrderCountMap(cancellationToken);

        var projected = employees.Select(e =>
        {
            var roleMeta = Roles.FirstOrDefault(x => string.Equals(x.Name, e.Role, StringComparison.OrdinalIgnoreCase));
            var roleId = e.RoleId ?? (roleMeta.Id == 0 ? 999 : roleMeta.Id);
            var roleDisplay = roleMeta.DisplayName ?? e.Role;
            var employeeCode = string.IsNullOrWhiteSpace(e.EmployeeCode) ? $"EMP{e.Id:0000}" : e.EmployeeCode;
            var department = string.IsNullOrWhiteSpace(e.Department) ? ResolveDepartment(e.Role) : e.Department;
            var phone = e.Phone;
            var email = e.Email;
            var position = string.IsNullOrWhiteSpace(e.Position) ? ResolvePosition(e.Role) : e.Position;
            var hireDate = e.HireDate ?? DateTime.Today.AddDays(-30 * e.Id);

            return new EmployeeListItemViewModel
            {
                Id = e.Id,
                EmployeeCode = employeeCode,
                FullName = e.FullName,
                Phone = phone,
                Email = email,
                Department = department,
                Position = position,
                RoleId = roleId,
                RoleName = roleDisplay,
                Username = e.Username,
                HireDate = hireDate,
                Status = string.IsNullOrWhiteSpace(e.Status) ? (e.IsActive ? "ACTIVE" : "INACTIVE") : e.Status,
                AssignedOrderCount = assignedOrderCountMap.GetValueOrDefault(e.FullName, 0),
                CanView = hasPermission(EmployeePermissions.View),
                CanEdit = hasPermission(EmployeePermissions.Edit),
                CanToggleStatus = hasPermission(EmployeePermissions.Edit),
                CanResetPassword = hasPermission(EmployeePermissions.ResetPassword)
            };
        });

        var totalEmployees = projected.Count();
        var activeEmployees = projected.Count(x => string.Equals(x.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase));
        var inactiveEmployees = totalEmployees - activeEmployees;

        if (!string.IsNullOrWhiteSpace(normalizedFilter.Keyword))
        {
            var keyword = normalizedFilter.Keyword.Trim();
            projected = projected.Where(x =>
                x.EmployeeCode.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || x.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || (x.Phone?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false)
                || (x.Email?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false)
                || x.Username.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(normalizedFilter.Department))
        {
            projected = projected.Where(x => string.Equals(x.Department, normalizedFilter.Department, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(normalizedFilter.Position))
        {
            projected = projected.Where(x => x.Position?.Contains(normalizedFilter.Position, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        if (normalizedFilter.RoleId.HasValue)
        {
            projected = projected.Where(x => x.RoleId == normalizedFilter.RoleId.Value);
        }

        if (!string.IsNullOrWhiteSpace(normalizedFilter.Status))
        {
            projected = projected.Where(x => string.Equals(x.Status, normalizedFilter.Status, StringComparison.OrdinalIgnoreCase));
        }

        if (normalizedFilter.HireDateFrom.HasValue)
        {
            projected = projected.Where(x => x.HireDate.HasValue && x.HireDate.Value.Date >= normalizedFilter.HireDateFrom.Value.Date);
        }

        if (normalizedFilter.HireDateTo.HasValue)
        {
            projected = projected.Where(x => x.HireDate.HasValue && x.HireDate.Value.Date <= normalizedFilter.HireDateTo.Value.Date);
        }

        projected = ApplySort(projected, normalizedFilter.SortBy, normalizedFilter.SortDirection);

        var totalRecords = projected.Count();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalRecords / (double)normalizedFilter.PageSize));
        var currentPage = Math.Min(normalizedFilter.Page, totalPages);

        var items = projected
            .Skip((currentPage - 1) * normalizedFilter.PageSize)
            .Take(normalizedFilter.PageSize)
            .ToList();

        normalizedFilter.Page = currentPage;

        return new EmployeeListViewModel
        {
            Filter = normalizedFilter,
            Items = items,
            TotalRecords = totalRecords,
            TotalPages = totalPages,
            TotalEmployees = totalEmployees,
            ActiveEmployees = activeEmployees,
            InactiveEmployees = inactiveEmployees,
            Departments = ["IT", "Sales", "Warehouse", "Production", "Finance", "Management"],
            Roles = Roles.Select(x => (x.Id, x.Code)).ToList(),
            CanCreateEmployee = hasPermission(EmployeePermissions.Create),
            CanExport = hasPermission(EmployeePermissions.View)
        };
    }

    public async Task<(bool Success, string? Error)> ToggleStatusAsync(long id, ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        var currentUserIdText = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (long.TryParse(currentUserIdText, out var currentUserId) && currentUserId == id)
        {
            return (false, "Không thể tự khóa chính tài khoản của mình.");
        }

        var employee = await dbContext.Employees.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (employee is null)
        {
            return (false, "Không tìm thấy nhân viên.");
        }

        if (employee.IsActive && string.Equals(employee.Role, DashboardRoles.Admin, StringComparison.OrdinalIgnoreCase))
        {
            var activeAdminCount = await dbContext.Employees.CountAsync(x => x.IsActive && x.Role == DashboardRoles.Admin, cancellationToken);
            if (activeAdminCount <= 1)
            {
                return (false, "Không thể khóa admin cuối cùng.");
            }
        }

        employee.IsActive = !employee.IsActive;
        await dbContext.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public Task<EmployeeCreatePageViewModel> BuildCreatePageAsync(EmployeeCreateViewModel? input = null, CancellationToken cancellationToken = default)
    {
        var vm = new EmployeeCreatePageViewModel
        {
            Employee = input ?? new EmployeeCreateViewModel(),
            Roles = Roles.Select(x => (x.Id, x.Code)).ToList()
        };

        return Task.FromResult(vm);
    }

    public async Task<EmployeeCreateResult> CreateAsync(EmployeeCreateViewModel model, ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        var normalizedEmployeeCode = model.EmployeeCode.Trim();
        var normalizedUsername = model.Username.Trim();
        var normalizedPhone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
        var normalizedEmail = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        var normalizedStatus = model.Status.Trim().ToUpperInvariant();

        var result = new EmployeeCreateResult();
        var roleMeta = Roles.FirstOrDefault(x => x.Id == model.RoleId);
        if (roleMeta.Id == 0)
        {
            result.Errors[nameof(EmployeeCreateViewModel.RoleId)] = "Vui lòng chọn vai trò.";
            return result;
        }

        if (await dbContext.Employees.AnyAsync(x => x.EmployeeCode == normalizedEmployeeCode, cancellationToken))
        {
            result.Errors[nameof(EmployeeCreateViewModel.EmployeeCode)] = "Mã nhân viên đã tồn tại.";
        }

        if (await dbContext.Employees.AnyAsync(x => x.Username == normalizedUsername, cancellationToken))
        {
            result.Errors[nameof(EmployeeCreateViewModel.Username)] = "Tên đăng nhập đã tồn tại.";
        }

        if (!string.IsNullOrWhiteSpace(normalizedPhone)
            && await dbContext.Employees.AnyAsync(x => x.Phone == normalizedPhone, cancellationToken))
        {
            result.Errors[nameof(EmployeeCreateViewModel.Phone)] = "Số điện thoại đã tồn tại.";
        }

        if (!string.IsNullOrWhiteSpace(normalizedEmail)
            && await dbContext.Employees.AnyAsync(x => x.Email == normalizedEmail, cancellationToken))
        {
            result.Errors[nameof(EmployeeCreateViewModel.Email)] = "Email đã tồn tại.";
        }

        if (result.Errors.Count > 0)
        {
            return result;
        }

        var employee = new ErpEmployee
        {
            EmployeeCode = normalizedEmployeeCode,
            FullName = model.FullName.Trim(),
            Phone = normalizedPhone,
            Email = normalizedEmail,
            Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim(),
            Department = string.IsNullOrWhiteSpace(model.Department) ? null : model.Department.Trim(),
            Position = string.IsNullOrWhiteSpace(model.Position) ? null : model.Position.Trim(),
            HireDate = model.HireDate?.Date,
            RoleId = roleMeta.Id,
            Role = roleMeta.Name,
            Status = normalizedStatus,
            Username = normalizedUsername,
            IsActive = normalizedStatus == "ACTIVE"
        };
        employee.PasswordHash = PasswordHasher.HashPassword(employee, model.Password);

        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync(cancellationToken);

        result.Success = true;
        result.EmployeeId = employee.Id;
        return result;
    }

    private static EmployeeFilterViewModel NormalizeFilter(EmployeeFilterViewModel filter)
    {
        filter.Page = filter.Page <= 0 ? 1 : filter.Page;
        filter.PageSize = filter.PageSize is 10 or 20 or 50 or 100 ? filter.PageSize : 20;
        filter.SortBy = string.IsNullOrWhiteSpace(filter.SortBy) ? "employeeCode" : filter.SortBy;
        filter.SortDirection = string.Equals(filter.SortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

        filter.Department = NormalizeOption(filter.Department);
        filter.Status = NormalizeOption(filter.Status);

        return filter;
    }

    private static string? NormalizeOption(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || string.Equals(value, "ALL", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return value.Trim();
    }

    private static IEnumerable<EmployeeListItemViewModel> ApplySort(IEnumerable<EmployeeListItemViewModel> query, string? sortBy, string? sortDirection)
    {
        var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "fullname" => desc ? query.OrderByDescending(x => x.FullName) : query.OrderBy(x => x.FullName),
            "department" => desc ? query.OrderByDescending(x => x.Department) : query.OrderBy(x => x.Department),
            "role" => desc ? query.OrderByDescending(x => x.RoleName) : query.OrderBy(x => x.RoleName),
            "hiredate" => desc ? query.OrderByDescending(x => x.HireDate) : query.OrderBy(x => x.HireDate),
            "status" => desc ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            _ => desc ? query.OrderByDescending(x => x.EmployeeCode) : query.OrderBy(x => x.EmployeeCode)
        };
    }

    private static string ResolveDepartment(string role) => role.ToLowerInvariant() switch
    {
        "admin" or "manager" => "Management",
        "sales" => "Sales",
        "warehouse" => "Warehouse",
        "production" => "Production",
        "accountant" => "Finance",
        _ => "IT"
    };

    private static string ResolvePosition(string role) => role.ToLowerInvariant() switch
    {
        "admin" => "Administrator",
        "manager" => "Operations Manager",
        "sales" => "Sales Executive",
        "warehouse" => "Warehouse Staff",
        "production" => "Production Staff",
        "accountant" => "Accountant",
        _ => "Employee"
    };

    private async Task<Dictionary<string, int>> BuildAssignedOrderCountMap(CancellationToken cancellationToken)
    {
        var payloads = await dbContext.Orders.AsNoTracking().Select(x => x.Payload).ToListAsync(cancellationToken);
        var names = payloads
            .Select(OrderPayloadMapper.Deserialize)
            .Where(x => x is not null)
            .Select(x => x!.AssignedEmployeeName)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim());

        return names
            .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.Count(), StringComparer.OrdinalIgnoreCase);
    }
}
