namespace Dealership.Api.Domain;           //defines how OTP codes are stored and used in the system.

public static class OtpPurpose
{
    public const string Register = "Register";
    public const string Login = "Login";
    public const string PurchaseRequest = "PurchaseRequest";
    public const string UpdateVehicle = "UpdateVehicle";
}