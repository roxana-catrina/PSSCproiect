using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proiect.Data.Models
{
    // Product table for stock management
    public class Product
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public int StockQuantity { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    // Orders table
    public class Order
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string CustomerName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string CustomerEmail { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string DeliveryStreet { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string DeliveryCity { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string DeliveryPostalCode { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string DeliveryCountry { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Unvalidated"; // Unvalidated, Validated, Confirmed, Paid, Invalid
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        
        // Navigation properties
        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public virtual Invoice? Invoice { get; set; }
        public virtual Package? Package { get; set; }
    }

    // Order items table
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int OrderId { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        public int Quantity { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }
        
        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;
        
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }

    // Invoices table
    public class Invoice
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;
        
        [Required]
        public int OrderId { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubtotalAmount { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal VatAmount { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalWithVat { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Unvalidated"; // Unvalidated, Validated, Generated, Invalid
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? GeneratedAt { get; set; }
        
        // Navigation property
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;
    }

    // Packages table
    public class Package
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int OrderId { get; set; }
        
        [StringLength(50)]
        public string? AWBNumber { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Unvalidated"; // Unvalidated, Validated, AWBAssigned, Shipped, Delivered, Invalid
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        
        // Navigation property
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;
    }
}
