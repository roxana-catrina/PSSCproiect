namespace Proiect.Domain.Models.Events;

/// <summary>
/// DTO for publishing InvoiceGenerated events to message bus
/// Contains only serializable primitive types
/// </summary>
public class InvoiceGeneratedDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalWithVat { get; set; }
    public DateTime GeneratedAt { get; set; }
}

