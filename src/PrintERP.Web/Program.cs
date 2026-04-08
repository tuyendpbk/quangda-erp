using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PrintERP.Web.Data;
using PrintERP.Web.Models.ViewModels;
using PrintERP.Web.Services.Auth;
using PrintERP.Web.Services.Implementations;
using PrintERP.Web.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
builder.Services.AddScoped<DbSeedService>();
builder.Services.AddScoped<IAuthService, InMemoryAuthService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

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
    options.AddPolicy("OrderStatusUpdate", policy =>
        policy.RequireClaim("Permission", OrderPermissions.StatusUpdate));
    options.AddPolicy("EmployeeView", policy =>
        policy.RequireClaim("Permission", EmployeePermissions.View));
    options.AddPolicy("EmployeeCreate", policy =>
        policy.RequireClaim("Permission", EmployeePermissions.Create));
    options.AddPolicy("EmployeeEdit", policy =>
        policy.RequireClaim("Permission", EmployeePermissions.Edit));
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

using (var scope = app.Services.CreateScope())
{
    var seedService = scope.ServiceProvider.GetRequiredService<DbSeedService>();
    await seedService.EnsureInitializedAsync();
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
