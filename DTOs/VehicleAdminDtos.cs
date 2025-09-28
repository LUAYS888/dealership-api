namespace Dealership.Api.DTOs;

public record VehicleCreateDto(
    string Vin, string Make, string Model, int Year, decimal Price,
    int? Mileage, string? BodyType, string? Color);

public record VehicleUpdateDto(
    string? Make, string? Model, int? Year, decimal? Price,
    int? Mileage, string? BodyType, string? Color, bool? IsAvailable);
