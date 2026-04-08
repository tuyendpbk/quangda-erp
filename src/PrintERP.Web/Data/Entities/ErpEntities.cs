using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrintERP.Web.Data.Entities;

[Table("erp_employees")]
public class ErpEmployee
{
    [Key]
    public int Id { get; set; }
    [Required, MaxLength(128)]
    public string Username { get; set; } = string.Empty;
    [Required, MaxLength(256)]
    public string FullName { get; set; } = string.Empty;
    [Required, MaxLength(64)]
    public string Role { get; set; } = string.Empty;
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

[Table("erp_customers")]
public class ErpCustomer
{
    [Key]
    public long Id { get; set; }
    [Required, MaxLength(64)]
    public string Code { get; set; } = string.Empty;
    [Required, MaxLength(256)]
    public string Name { get; set; } = string.Empty;
    [Required, MaxLength(64)]
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxCode { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal CurrentDebt { get; set; }
}

[Table("erp_product_categories")]
public class ErpProductCategory
{
    [Key]
    public long Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string DefaultUnit { get; set; } = string.Empty;
    public bool UseAreaPricing { get; set; }
}

[Table("erp_orders")]
public class ErpOrder
{
    [Key]
    public long Id { get; set; }
    [Required]
    public string OrderCode { get; set; } = string.Empty;
    [Required]
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

[Table("erp_dashboard_materials")]
public class ErpDashboardMaterial
{
    [Key]
    public long Id { get; set; }
    [Required]
    public string MaterialCode { get; set; } = string.Empty;
    [Required]
    public string MaterialName { get; set; } = string.Empty;
    [Required]
    public string GroupName { get; set; } = string.Empty;
    [Column(TypeName = "numeric(18,2)")]
    public decimal CurrentStock { get; set; }
    [Column(TypeName = "numeric(18,2)")]
    public decimal MinStockLevel { get; set; }
    [Required]
    public string Unit { get; set; } = string.Empty;
}

[Table("erp_dashboard_payments")]
public class ErpDashboardPayment
{
    [Key]
    public long Id { get; set; }
    public DateTime PaymentDate { get; set; }
    [Required]
    public string OrderCode { get; set; } = string.Empty;
    [Required]
    public string CustomerName { get; set; } = string.Empty;
    [Column(TypeName = "numeric(18,2)")]
    public decimal Amount { get; set; }
    [Required]
    public string Method { get; set; } = string.Empty;
}
