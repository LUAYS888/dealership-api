namespace Dealership.Api.Domain;

public static class PurchaseRequestStatus
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";           // optional
    public const string Declined = "Declined";           // optional
    public const string ConvertedToSale = "ConvertedToSale";
}

public class PurchaseRequest
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
    public int VehicleId { get; set; }
    public string Status { get; set; } = PurchaseRequestStatus.Pending;
    public string? Notes { get; set; }
    public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public Vehicle? Vehicle { get; set; }
}
