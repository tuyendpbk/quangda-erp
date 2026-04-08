using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintERP.Web.Models.ViewModels;
using PrintERP.Web.Services.Auth;

namespace PrintERP.Web.Controllers;

[AllowAnonymous]
public class AccountController(IAuthService authService, ILogger<AccountController> logger) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        model.Username = model.Username.Trim();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var loginResult = await authService.AuthenticateAsync(model.Username, model.Password, cancellationToken);

            if (loginResult.IsInactiveAccount)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa hoặc chưa được kích hoạt.");
                return View(model);
            }

            if (!loginResult.IsSuccess || loginResult.User is null)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, loginResult.User.UserId.ToString()),
                new(ClaimTypes.Name, loginResult.User.Username),
                new("FullName", loginResult.User.FullName),
                new(ClaimTypes.Role, loginResult.User.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(14)
                    : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            logger.LogInformation("Login success for username {Username} at {TimestampUtc}.", model.Username, DateTime.UtcNow);

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Login failed due to server error for username {Username} at {TimestampUtc}.", model.Username, DateTime.UtcNow);
            ModelState.AddModelError(string.Empty, "Hệ thống đang bận, vui lòng thử lại sau.");
            return View(model);
        }
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();
}
