namespace Dealership.Api.Domain;

public class OtpCode
{
    public int Id { get; set; }

    // For Register, the user doesn't exist yet → keep Email.
    public string? UserId { get; set; }
    public string? Email { get; set; }

    public string Purpose { get; set; } = default!;   // one of OtpPurpose.*
    public string Code { get; set; } = default!;      // e.g., 6 digits
    public DateTime ExpiresUtc { get; set; }          // e.g., now + 5 min
    public DateTime? ConsumedUtc { get; set; }        // set after success
    public int Attempts { get; set; }                 // increment on tries
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public bool IsActive => ConsumedUtc is null && DateTime.UtcNow <= ExpiresUtc;  //The OTP is valid when it was not used before, and not expired. 
}