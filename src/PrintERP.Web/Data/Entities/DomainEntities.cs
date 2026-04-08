using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrintERP.Web.Data.Entities;

public abstract class AuditableEntity
{
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
}

[Table("roles")]
public class Role : AuditableEntity
{
    [Key]
    public long Id { get; set; }
    [Required, MaxLength(100)]
    public string Code { get; set; } = string.Empty;
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(500)]
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<MenuRole> MenuRoles { get; set; } = new List<MenuRole>();
}

[Table("permissions")]
public class Permission : AuditableEntity
{
    [Key]
    public long Id { get; set; }
    [Required, MaxLength(100)]
    public string Code { get; set; } = string.Empty;
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(500)]
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

[Table("role_permissions")]
public class RolePermission
{
    [Key]
    public long Id { get; set; }
    [Required]
    public long RoleId { get; set; }
    [Required]
    public long PermissionId { get; set; }

    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}

[Table("employees")]
public class Employee : AuditableEntity
{
    [Key]
    public long Id { get; set; }
    [Required, MaxLength(50)]
    public string EmployeeCode { get; set; } = string.Empty;
    [Required, MaxLength(128)]
    public string Username { get; set; } = string.Empty;
    [Required, MaxLength(256)]
    public string FullName { get; set; } = string.Empty;
    [MaxLength(20)]
    public string? Phone { get; set; }
    [MaxLength(150)]
    public string? Email { get; set; }
    [MaxLength(500)]
    public string? Address { get; set; }
    [MaxLength(100)]
    public string? Department { get; set; }
    [MaxLength(100)]
    public string? Position { get; set; }
    public DateOnly? HireDate { get; set; }
    [Required]
    public long RoleId { get; set; }
    [Required, MaxLength(20)]
    public string Status { get; set; } = "ACTIVE";
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }

    public Role Role { get; set; } = null!;
    public ICollection<Order> SalesOrders { get; set; } = new List<Order>();
    public ICollection<Order> AssignedOrders { get; set; } = new List<Order>();
}

[Table("customers")]
public class Customer : AuditableEntity
{
    [Key]
    public long Id { get; set; }
    [Required, MaxLength(50)]
    public string CustomerCode { get; set; } = string.Empty;
    [Required, MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(50)]
    public string? CustomerType { get; set; }
    [MaxLength(255)]
    public string? ContactName { get; set; }
    [MaxLength(20)]
    public string? Phone { get; set; }
    [MaxLength(150)]
    public string? Email { get; set; }
    [MaxLength(500)]
    public string? Address { get; set; }
    [MaxLength(20)]
    public string? TaxCode { get; set; }
    [MaxLength(1000)]
    public string? Notes { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal CurrentDebt { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

[Table("product_categories")]
public class ProductCategory : AuditableEntity
{
    [Key]
    public long Id { get; set; }
    [Required, MaxLength(100)]
    public string Code { get; set; } = string.Empty;
    [Required, MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(500)]
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

[Table("material_categories")]
public class MaterialCategory : AuditableEntity
{
    [Key]
    public long Id { get; set; }
    [Required, MaxLength(100)]
    public string Code { get; set; } = string.Empty;
    [Required, MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(500)]
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Material> Materials { get; set; } = new List<Material>();
}

[Table("units")]
public class Unit : AuditableEntity
{
    [Key]
    public long Id { get; set; }
    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Material> Materials { get; set; } = new List<Material>();
}

[Table("suppliers")]
public class Supplier : AuditableEntity
{
    [Key]
    public long Id { get; set; }
    [Required, MaxLength(100)]
    public string SupplierCode { get; set; } = string.Empty;
    [Required, MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(255)]
    public string? ContactName { get; set; }
    [MaxLength(20)]
    public string? Phone { get; set; }
    [MaxLength(150)]
    public string? Email { get; set; }
    [MaxLength(500)]
    public string? Address { get; set; }
    [MaxLength(1000)]
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<StockIn> StockIns { get; set; } = new List<StockIn>();
}

[Table("menus")]
public class Menu : AuditableEntity
{
    [Key]
    public long Id { get; set; }
    [Required, MaxLength(100)]
    public string Code { get; set; } = string.Empty;
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    public long? ParentId { get; set; }
    [MaxLength(50)]
    public string? Area { get; set; }
    [MaxLength(100)]
    public string? Controller { get; set; }
    [MaxLength(100)]
    public string? Action { get; set; }
    [MaxLength(500)]
    public string? StaticUrl { get; set; }
    [Column(TypeName = "jsonb")]
    public string? RouteValuesJson { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsVisible { get; set; } = true;

    public Menu? Parent { get; set; }
    public ICollection<Menu> Children { get; set; } = new List<Menu>();
    public ICollection<MenuRole> MenuRoles { get; set; } = new List<MenuRole>();
}

[Table("menu_roles")]
public class MenuRole
{
    [Key]
    public long Id { get; set; }
    [Required]
    public long MenuId { get; set; }
    [Required]
    public long RoleId { get; set; }

    public Menu Menu { get; set; } = null!;
    public Role Role { get; set; } = null!;
}

[Table("materials")]
public class Material : AuditableEntity
{
    [Key]
    public long Id { get; set; }
    [Required, MaxLength(50)]
    public string MaterialCode { get; set; } = string.Empty;
    [Required, MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    [Required]
    public long MaterialCategoryId { get; set; }
    [Required]
    public long UnitId { get; set; }
    [Column(TypeName = "numeric(18,4)")]
    public decimal OpeningStock { get; set; }
    [Column(TypeName = "numeric(18,4)")]
    public decimal CurrentStock { get; set; }
    [Column(TypeName = "numeric(18,4)")]
    public decimal MinStockLevel { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal AverageCost { get; set; }
    [MaxLength(255)]
    public string? Specification { get; set; }
    [MaxLength(20)]
    public string Status { get; set; } = "ACTIVE";

    public MaterialCategory MaterialCategory { get; set; } = null!;
    public Unit Unit { get; set; } = null!;
}

[Table("price_lists")]
public class PriceList : AuditableEntity
{
    [Key]
    public long Id { get; set; }
    [Required, MaxLength(100)]
    public string Code { get; set; } = string.Empty;
    [Required, MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<PriceListItem> Items { get; set; } = new List<PriceListItem>();
}

[Table("price_list_items")]
public class PriceListItem : AuditableEntity
{
    [Key]
    public long Id { get; set; }
    [Required]
    public long PriceListId { get; set; }
    public long? ProductCategoryId { get; set; }
    [MaxLength(100)]
    public string? PrintType { get; set; }
    [MaxLength(100)]
    public string? MaterialType { get; set; }
    [MaxLength(100)]
    public string? FinishingType { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal BasePrice { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal MarkupPercent { get; set; }

    public PriceList PriceList { get; set; } = null!;
    public ProductCategory? ProductCategory { get; set; }
}

[Table("product_material_recipes")]
public class ProductMaterialRecipe : AuditableEntity
{
    [Key]
    public long Id { get; set; }
    [Required]
    public long ProductCategoryId { get; set; }
    [Required]
    public long MaterialId { get; set; }
    [Column(TypeName = "numeric(18,4)")]
    public decimal QuantityPerUnit { get; set; }
    [Column(TypeName = "numeric(18,4)")]
    public decimal WasteRatePercent { get; set; }

    public ProductCategory ProductCategory { get; set; } = null!;
    public Material Material { get; set; } = null!;
}

[Table("orders")]
public class Order : AuditableEntity
{
    [Key]
    public long Id { get; set; }
    [Required, MaxLength(50)]
    public string OrderCode { get; set; } = string.Empty;
    [Required]
    public long CustomerId { get; set; }
    [Required]
    public long SalesEmployeeId { get; set; }
    public long? AssignedEmployeeId { get; set; }
    public DateOnly OrderDate { get; set; }
    public DateOnly? DeliveryDate { get; set; }
    [MaxLength(1000)]
    public string? Notes { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal SubtotalAmount { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal DiscountAmount { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal TaxAmount { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal TotalAmount { get; set; }
    [Required, MaxLength(20)]
    public string Status { get; set; } = "NEW";
    [Required, MaxLength(20)]
    public string PaymentStatus { get; set; } = "UNPAID";

    public Customer Customer { get; set; } = null!;
    public Employee SalesEmployee { get; set; } = null!;
    public Employee? AssignedEmployee { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<OrderStatusHistory> StatusHistories { get; set; } = new List<OrderStatusHistory>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

[Table("order_items")]
public class OrderItem : AuditableEntity
{
    [Key]
    public long Id { get; set; }
    [Required]
    public long OrderId { get; set; }
    public long? ProductCategoryId { get; set; }
    [Required, MaxLength(255)]
    public string ItemName { get; set; } = string.Empty;
    [MaxLength(2000)]
    public string? Description { get; set; }
    [Column(TypeName = "numeric(18,4)")]
    public decimal Width { get; set; }
    [Column(TypeName = "numeric(18,4)")]
    public decimal Height { get; set; }
    [MaxLength(50)]
    public string Unit { get; set; } = "m2";
    [Column(TypeName = "numeric(18,4)")]
    public decimal Quantity { get; set; }
    [Column(TypeName = "numeric(18,4)")]
    public decimal Area { get; set; }
    [MaxLength(255)]
    public string? MaterialDescription { get; set; }
    [MaxLength(100)]
    public string? PrintType { get; set; }
    [MaxLength(100)]
    public string? FinishingType { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal EstimatedUnitPrice { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal FinalUnitPrice { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal EstimatedLineTotal { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal FinalLineTotal { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal EstimatedCost { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal EstimatedProfit { get; set; }
    [Column(TypeName = "jsonb")]
    public string? EstimateBreakdownJson { get; set; }

    public Order Order { get; set; } = null!;
    public ProductCategory? ProductCategory { get; set; }
    public ICollection<MaterialUsage> MaterialUsages { get; set; } = new List<MaterialUsage>();
}

[Table("order_status_histories")]
public class OrderStatusHistory : AuditableEntity
{
    [Key]
    public long Id { get; set; }
    [Required]
    public long OrderId { get; set; }
    [Required, MaxLength(20)]
    public string OldStatus { get; set; } = string.Empty;
    [Required, MaxLength(20)]
    public string NewStatus { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string? Notes { get; set; }

    public Order Order { get; set; } = null!;
}

[Table("payments")]
public class Payment : AuditableEntity
{
    [Key]
    public long Id { get; set; }
    [Required]
    public long OrderId { get; set; }
    public DateOnly PaymentDate { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal Amount { get; set; }
    [Required, MaxLength(50)]
    public string Method { get; set; } = string.Empty;
    [MaxLength(100)]
    public string? ReferenceCode { get; set; }
    [MaxLength(1000)]
    public string? Notes { get; set; }

    public Order Order { get; set; } = null!;
}

[Table("stock_in")]
public class StockIn : AuditableEntity
{
    [Key]
    public long Id { get; set; }
    [Required, MaxLength(50)]
    public string StockInCode { get; set; } = string.Empty;
    public long? SupplierId { get; set; }
    public DateOnly StockInDate { get; set; }
    [MaxLength(1000)]
    public string? Notes { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal TotalAmount { get; set; }

    public Supplier? Supplier { get; set; }
    public ICollection<StockInItem> Items { get; set; } = new List<StockInItem>();
}

[Table("stock_in_items")]
public class StockInItem : AuditableEntity
{
    [Key]
    public long Id { get; set; }
    [Required]
    public long StockInId { get; set; }
    [Required]
    public long MaterialId { get; set; }
    [Column(TypeName = "numeric(18,4)")]
    public decimal Quantity { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal UnitPrice { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal LineTotal { get; set; }

    public StockIn StockIn { get; set; } = null!;
    public Material Material { get; set; } = null!;
}

[Table("stock_out")]
public class StockOut : AuditableEntity
{
    [Key]
    public long Id { get; set; }
    [Required, MaxLength(50)]
    public string StockOutCode { get; set; } = string.Empty;
    public DateOnly StockOutDate { get; set; }
    public long? OrderId { get; set; }
    [MaxLength(255)]
    public string? Purpose { get; set; }
    [MaxLength(1000)]
    public string? Notes { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal TotalAmount { get; set; }

    public Order? Order { get; set; }
    public ICollection<StockOutItem> Items { get; set; } = new List<StockOutItem>();
}

[Table("stock_out_items")]
public class StockOutItem : AuditableEntity
{
    [Key]
    public long Id { get; set; }
    [Required]
    public long StockOutId { get; set; }
    [Required]
    public long MaterialId { get; set; }
    [Column(TypeName = "numeric(18,4)")]
    public decimal Quantity { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal UnitCost { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal LineTotal { get; set; }

    public StockOut StockOut { get; set; } = null!;
    public Material Material { get; set; } = null!;
}

[Table("material_usages")]
public class MaterialUsage : AuditableEntity
{
    [Key]
    public long Id { get; set; }
    [Required]
    public long OrderItemId { get; set; }
    [Required]
    public long MaterialId { get; set; }
    [Column(TypeName = "numeric(18,4)")]
    public decimal PlannedQuantity { get; set; }
    [Column(TypeName = "numeric(18,4)")]
    public decimal ActualQuantity { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal UnitCost { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal TotalCost { get; set; }

    public OrderItem OrderItem { get; set; } = null!;
    public Material Material { get; set; } = null!;
}

[Table("report_exports")]
public class ReportExport : AuditableEntity
{
    [Key]
    public long Id { get; set; }
    [Required, MaxLength(100)]
    public string ReportType { get; set; } = string.Empty;
    [Required, MaxLength(255)]
    public string FileName { get; set; } = string.Empty;
    [Required, MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;
    [Column(TypeName = "jsonb")]
    public string? FilterJson { get; set; }
}

[Table("revenue_forecasts")]
public class RevenueForecast : AuditableEntity
{
    [Key]
    public long Id { get; set; }
    [Required, MaxLength(20)]
    public string ForecastType { get; set; } = string.Empty;
    public int ForecastYear { get; set; }
    public int ForecastPeriod { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal PredictedRevenue { get; set; }
    [Required, MaxLength(100)]
    public string ModelName { get; set; } = string.Empty;
    [Column(TypeName = "jsonb")]
    public string? InputSummaryJson { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal? ActualRevenue { get; set; }
}
