namespace Dealership.Api.DTOs;                                       //Define the shape of data the API sends/receives for authentication.

public record RegisterStartDto(string Email, string Password);        //Record = data class            Sent when a user starts registration.
public record RegisterVerifyDto(string Email, string OtpCode);          //Sent when a user submits the OTP to confirm registration.

public record LoginStartDto(string Email, string Password);        //Sent when a user tries to log in with Email + Password.
public record LoginVerifyDto(string Email, string OtpCode);            //Sent when the user enters the OTP they received.

public record AuthResultDto(string Token, string Role, string Email);        //Returned by the API after successful login/registration.
