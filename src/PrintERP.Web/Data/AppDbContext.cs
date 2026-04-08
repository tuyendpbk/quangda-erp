using Microsoft.EntityFrameworkCore;
using PrintERP.Web.Data.Entities;

namespace PrintERP.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ErpEmployee> Employees => Set<ErpEmployee>();
    public DbSet<ErpCustomer> Customers => Set<ErpCustomer>();
    public DbSet<ErpProductCategory> ProductCategories => Set<ErpProductCategory>();
    public DbSet<ErpOrder> Orders => Set<ErpOrder>();
    public DbSet<ErpDashboardMaterial> DashboardMaterials => Set<ErpDashboardMaterial>();
    public DbSet<ErpDashboardPayment> DashboardPayments => Set<ErpDashboardPayment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ErpOrder>().Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<ErpOrder>().HasIndex(x => x.OrderCode).IsUnique();
        modelBuilder.Entity<ErpCustomer>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<ErpEmployee>().HasIndex(x => x.Username).IsUnique();
        modelBuilder.Entity<ErpOrder>().Property(x => x.Payload).HasColumnType("jsonb");
    }
}
