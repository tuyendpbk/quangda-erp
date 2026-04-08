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
        var admin = new EmployeeRecord
        {
            Id = 1,
            Username = "admin",
            FullName = "System Administrator",
            Role = "Admin",
            IsActive = true,
            PasswordHash = string.Empty
        };

        admin.PasswordHash = PasswordHasher.HashPassword(admin, "Admin@123");

        var inactive = new EmployeeRecord
        {
            Id = 2,
            Username = "warehouse",
            FullName = "Warehouse User",
            Role = "Warehouse",
            IsActive = false,
            PasswordHash = string.Empty
        };

        inactive.PasswordHash = PasswordHasher.HashPassword(inactive, "Warehouse@123");

        return [admin, inactive];
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
