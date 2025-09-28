namespace Dealership.Api.Domain;

public class Vehicle
{
    public int Id { get; set; }
    public string Vin { get; set; } = default!;
    public string Make { get; set; } = default!;
    public string Model { get; set; } = default!;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public int? Mileage { get; set; }
    public string? BodyType { get; set; }
    public string? Color { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
