using Microsoft.EntityFrameworkCore;
using PrintERP.Web.Data.Entities;

namespace PrintERP.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Legacy MVP tables
    public DbSet<ErpEmployee> Employees => Set<ErpEmployee>();
    public DbSet<ErpCustomer> Customers => Set<ErpCustomer>();
    public DbSet<ErpProductCategory> ProductCategories => Set<ErpProductCategory>();
    public DbSet<ErpOrder> Orders => Set<ErpOrder>();
    public DbSet<ErpDashboardMaterial> DashboardMaterials => Set<ErpDashboardMaterial>();
    public DbSet<ErpDashboardPayment> DashboardPayments => Set<ErpDashboardPayment>();

    // SRS master tables
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Employee> EmployeeProfiles => Set<Employee>();
    public DbSet<Customer> CustomerProfiles => Set<Customer>();
    public DbSet<ProductCategory> ProductCategoryMasters => Set<ProductCategory>();
    public DbSet<MaterialCategory> MaterialCategories => Set<MaterialCategory>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<MenuRole> MenuRoles => Set<MenuRole>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<PriceListItem> PriceListItems => Set<PriceListItem>();
    public DbSet<ProductMaterialRecipe> ProductMaterialRecipes => Set<ProductMaterialRecipe>();

    // SRS transaction tables
    public DbSet<Order> OrderHeaders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<StockIn> StockIns => Set<StockIn>();
    public DbSet<StockInItem> StockInItems => Set<StockInItem>();
    public DbSet<StockOut> StockOuts => Set<StockOut>();
    public DbSet<StockOutItem> StockOutItems => Set<StockOutItem>();
    public DbSet<MaterialUsage> MaterialUsages => Set<MaterialUsage>();
    public DbSet<ReportExport> ReportExports => Set<ReportExport>();
    public DbSet<RevenueForecast> RevenueForecasts => Set<RevenueForecast>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureLegacyEntities(modelBuilder);
        ConfigureSrsEntities(modelBuilder);
    }

    private static void ConfigureLegacyEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ErpOrder>().Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<ErpOrder>().HasIndex(x => x.OrderCode).IsUnique();
        modelBuilder.Entity<ErpCustomer>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<ErpEmployee>().HasIndex(x => x.EmployeeCode).IsUnique();
        modelBuilder.Entity<ErpEmployee>().HasIndex(x => x.Username).IsUnique();
        modelBuilder.Entity<ErpEmployee>().HasIndex(x => x.Phone).IsUnique();
        modelBuilder.Entity<ErpEmployee>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<ErpOrder>().Property(x => x.Payload).HasColumnType("jsonb");
    }

    private static void ConfigureSrsEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Permission>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Employee>().HasIndex(x => x.EmployeeCode).IsUnique();
        modelBuilder.Entity<Employee>().HasIndex(x => x.Username).IsUnique();
        modelBuilder.Entity<Customer>().HasIndex(x => x.CustomerCode).IsUnique();
        modelBuilder.Entity<ProductCategory>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<MaterialCategory>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Unit>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Supplier>().HasIndex(x => x.SupplierCode).IsUnique();
        modelBuilder.Entity<Menu>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Material>().HasIndex(x => x.MaterialCode).IsUnique();
        modelBuilder.Entity<PriceList>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Order>().HasIndex(x => x.OrderCode).IsUnique();
        modelBuilder.Entity<StockIn>().HasIndex(x => x.StockInCode).IsUnique();
        modelBuilder.Entity<StockOut>().HasIndex(x => x.StockOutCode).IsUnique();

        modelBuilder.Entity<RolePermission>()
            .HasIndex(x => new { x.RoleId, x.PermissionId })
            .IsUnique();
        modelBuilder.Entity<MenuRole>()
            .HasIndex(x => new { x.MenuId, x.RoleId })
            .IsUnique();

        modelBuilder.Entity<Menu>()
            .HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Employee>()
            .HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(x => x.Customer)
            .WithMany(x => x.Orders)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(x => x.SalesEmployee)
            .WithMany(x => x.SalesOrders)
            .HasForeignKey(x => x.SalesEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(x => x.AssignedEmployee)
            .WithMany(x => x.AssignedOrders)
            .HasForeignKey(x => x.AssignedEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderItem>()
            .HasOne(x => x.Order)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderStatusHistory>()
            .HasOne(x => x.Order)
            .WithMany(x => x.StatusHistories)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Payment>()
            .HasOne(x => x.Order)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StockInItem>()
            .HasOne(x => x.StockIn)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.StockInId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StockOutItem>()
            .HasOne(x => x.StockOut)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.StockOutId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MaterialUsage>()
            .HasOne(x => x.OrderItem)
            .WithMany(x => x.MaterialUsages)
            .HasForeignKey(x => x.OrderItemId)
            .OnDelete(DeleteBehavior.Cascade);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(e => typeof(AuditableEntity).IsAssignableFrom(e.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType).Property(nameof(AuditableEntity.CreatedAt)).HasDefaultValueSql("now()");
            modelBuilder.Entity(entityType.ClrType).Property(nameof(AuditableEntity.UpdatedAt)).HasDefaultValueSql("now()");
        }
    }
}
