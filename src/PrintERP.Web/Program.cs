using Microsoft.AspNetCore.Authentication.Cookies;
using PrintERP.Web.Models.ViewModels;
using PrintERP.Web.Services.Auth;
using PrintERP.Web.Services.Implementations;
using PrintERP.Web.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, InMemoryAuthService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DashboardView", policy =>
        policy.RequireClaim("Permission", DashboardPermissions.View));
    options.AddPolicy("OrderCreate", policy =>
        policy.RequireClaim("Permission", OrderPermissions.Create));
    options.AddPolicy("OrderView", policy =>
        policy.RequireClaim("Permission", OrderPermissions.View));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
