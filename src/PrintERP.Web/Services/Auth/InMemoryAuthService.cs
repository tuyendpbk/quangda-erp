using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PrintERP.Web.Data;
using PrintERP.Web.Data.Entities;

namespace PrintERP.Web.Services.Auth;

public class InMemoryAuthService(AppDbContext dbContext) : IAuthService
{
    private static readonly PasswordHasher<ErpEmployee> PasswordHasher = new();

    public async Task<LoginResult> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var normalizedUsername = username.Trim().ToLower();
        var employee = await dbContext.Employees.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Username.ToLower() == normalizedUsername, cancellationToken);

        if (employee is null)
        {
            return LoginResult.InvalidCredentials();
        }

        if (!employee.IsActive)
        {
            return LoginResult.InactiveAccount();
        }

        var verifyResult = PasswordHasher.VerifyHashedPassword(employee, employee.PasswordHash, password);
        if (verifyResult == PasswordVerificationResult.Failed)
        {
            return LoginResult.InvalidCredentials();
        }

        return LoginResult.Success(new AuthenticatedUser
        {
            UserId = employee.Id,
            Username = employee.Username,
            FullName = employee.FullName,
            Role = employee.Role
        });
    }
}
