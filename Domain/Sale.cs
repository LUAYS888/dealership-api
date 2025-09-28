namespace Dealership.Api.Domain;          //persistent records for customers’ requests and completed sales.

public class Sale
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public string CustomerId { get; set; } = default!;
    public decimal FinalPrice { get; set; }
    public string ProcessedByAdminId { get; set; } = default!;
    public DateTime SoldAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsVoided { get; set; }

    public Vehicle? Vehicle { get; set; }
    public User? Customer { get; set; }
}
