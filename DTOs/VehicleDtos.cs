namespace Dealership.Api.DTOs;                   //To prevent oversharing. 

public record VehicleListItemDto(  //Light weight list.
    int Id, string Vin, string Make, string Model, int Year, decimal Price, bool IsAvailable);

public record VehicleDetailDto(      //Detailed list. 
    int Id, string Vin, string Make, string Model, int Year, decimal Price, int? Mileage,
    string? BodyType, string? Color, bool IsAvailable, DateTime CreatedUtc);
