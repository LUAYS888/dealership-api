namespace Dealership.Api.DTOs;

public record PurchaseRequestStartDto(int VehicleId, string? Notes);
public record PurchaseRequestConfirmDto(int VehicleId, string OtpCode);

public record PurchaseRequestItemDto(
    int Id, int VehicleId, string Make, string Model, int Year, decimal Price,
    string Status, DateTime RequestedAtUtc);

public record SaleCreateDto(int VehicleId, string CustomerEmail, decimal FinalPrice);
public record SaleItemDto(int Id, int VehicleId, decimal FinalPrice, DateTime SoldAtUtc, bool IsVoided);
