using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PrintERP.Web.Data;
using PrintERP.Web.Models.ViewModels;
using PrintERP.Web.Services.Interfaces;

namespace PrintERP.Web.Services.Implementations;

public class EmployeeService(AppDbContext dbContext) : IEmployeeService
{
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
            var roleId = roleMeta.Id == 0 ? 999 : roleMeta.Id;
            var roleDisplay = roleMeta.DisplayName ?? e.Role;
            var employeeCode = $"EMP{e.Id:0000}";
            var department = ResolveDepartment(e.Role);
            var phone = $"09{e.Id:00}{e.Id:00}{e.Id:00}{e.Id:00}"[..10];
            var email = $"{e.Username.ToLowerInvariant()}@printerp.local";
            var position = ResolvePosition(e.Role);
            var hireDate = DateTime.Today.AddDays(-30 * e.Id);

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
                Status = e.IsActive ? "ACTIVE" : "INACTIVE",
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
