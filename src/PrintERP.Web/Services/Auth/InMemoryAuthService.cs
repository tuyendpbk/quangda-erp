using Microsoft.AspNetCore.Identity;

namespace PrintERP.Web.Services.Auth;

public class InMemoryAuthService : IAuthService
{
    private static readonly PasswordHasher<EmployeeRecord> PasswordHasher = new();

    private static readonly IReadOnlyList<EmployeeRecord> Employees = BuildEmployees();

    public Task<LoginResult> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var normalizedUsername = username.Trim();
        var employee = Employees.FirstOrDefault(x => x.Username.Equals(normalizedUsername, StringComparison.OrdinalIgnoreCase));

        if (employee is null)
        {
            return Task.FromResult(LoginResult.InvalidCredentials());
        }

        if (!employee.IsActive)
        {
            return Task.FromResult(LoginResult.InactiveAccount());
        }

        var verifyResult = PasswordHasher.VerifyHashedPassword(employee, employee.PasswordHash, password);

        if (verifyResult == PasswordVerificationResult.Failed)
        {
            return Task.FromResult(LoginResult.InvalidCredentials());
        }

        return Task.FromResult(LoginResult.Success(new AuthenticatedUser
        {
            UserId = employee.Id,
            Username = employee.Username,
            FullName = employee.FullName,
            Role = employee.Role
        }));
    }

    private static IReadOnlyList<EmployeeRecord> BuildEmployees()
    {
        var employees = new List<EmployeeRecord>
        {
            new() { Id = 1, Username = "admin", FullName = "System Administrator", Role = "Admin", IsActive = true, PasswordHash = string.Empty },
            new() { Id = 2, Username = "manager", FullName = "Factory Manager", Role = "Manager", IsActive = true, PasswordHash = string.Empty },
            new() { Id = 3, Username = "sales", FullName = "Sales User", Role = "Sales", IsActive = true, PasswordHash = string.Empty },
            new() { Id = 4, Username = "warehouse", FullName = "Warehouse User", Role = "Warehouse", IsActive = true, PasswordHash = string.Empty },
            new() { Id = 5, Username = "accountant", FullName = "Accountant User", Role = "Accountant", IsActive = true, PasswordHash = string.Empty },
            new() { Id = 6, Username = "production", FullName = "Production User", Role = "Production", IsActive = true, PasswordHash = string.Empty },
            new() { Id = 7, Username = "inactive", FullName = "Inactive User", Role = "Warehouse", IsActive = false, PasswordHash = string.Empty }
        };

        foreach (var employee in employees)
        {
            employee.PasswordHash = PasswordHasher.HashPassword(employee, "Admin@123");
        }

        return employees;
    }

    private sealed class EmployeeRecord
    {
        public int Id { get; init; }

        public required string Username { get; init; }

        public required string FullName { get; init; }

        public required string Role { get; init; }

        public required string PasswordHash { get; set; }

        public required bool IsActive { get; init; }
    }
}
