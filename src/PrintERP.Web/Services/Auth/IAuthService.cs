namespace PrintERP.Web.Services.Auth;

public interface IAuthService
{
    Task<LoginResult> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
}

public sealed class LoginResult
{
    public bool IsSuccess { get; init; }

    public bool IsInactiveAccount { get; init; }

    public AuthenticatedUser? User { get; init; }

    public static LoginResult Success(AuthenticatedUser user) => new()
    {
        IsSuccess = true,
        User = user
    };

    public static LoginResult InvalidCredentials() => new();

    public static LoginResult InactiveAccount() => new()
    {
        IsInactiveAccount = true
    };
}

public sealed class AuthenticatedUser
{
    public required int UserId { get; init; }

    public required string Username { get; init; }

    public required string FullName { get; init; }

    public required string Role { get; init; }
}
