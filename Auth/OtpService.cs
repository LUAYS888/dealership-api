using Dealership.Api.Data;                                  //Handles generation, logging (simulated delivery), rate limiting, expiry, and consumption.
using Dealership.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Dealership.Api.Auth;

public class OtpService(AppDbContext db, ILogger<OtpService> log)
{
    private const int CodeLength = 6;
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);
    private const int MaxAttempts = 5;

    public async Task<OtpCode> GenerateForEmailAsync(string email, string purpose)
    {
        var code = Random.Shared.Next(0, (int)Math.Pow(10, CodeLength)).ToString($"D{CodeLength}");

        var otp = new OtpCode
        {
            Email = email,
            Purpose = purpose,
            Code = code,
            ExpiresUtc = DateTime.UtcNow.Add(Lifetime)
        };
        db.Otps.Add(otp);
        await db.SaveChangesAsync();

        log.LogInformation("OTP [{Purpose}] for {Email}: {Code} (expires {Expires})",
            purpose, email, code, otp.ExpiresUtc);
        Console.WriteLine($"OTP [{purpose}] for {(email ?? email)}: {code} (expires {otp.ExpiresUtc:O})");

        return otp;
    }

    public async Task<OtpCode> GenerateForUserAsync(string userId, string purpose)
    {
        var code = Random.Shared.Next(0, (int)Math.Pow(10, CodeLength)).ToString($"D{CodeLength}");
        var otp = new OtpCode
        {
            UserId = userId,
            Purpose = purpose,
            Code = code,
            ExpiresUtc = DateTime.UtcNow.Add(Lifetime)
        };
        db.Otps.Add(otp);
        await db.SaveChangesAsync();

        log.LogInformation("OTP [{Purpose}] for user {UserId}: {Code} (expires {Expires})",
            purpose, userId, code, otp.ExpiresUtc);

        return otp;
    }

    public async Task<bool> ValidateByEmailAsync(string email, string purpose, string code)
    {
        var otp = await db.Otps
            .Where(o => o.Email == email && o.Purpose == purpose && o.ConsumedUtc == null)
            .OrderByDescending(o => o.Id).FirstOrDefaultAsync();

        return await ValidateCoreAsync(otp, code);
    }

    public async Task<bool> ValidateByUserAsync(string userId, string purpose, string code)
    {
        var otp = await db.Otps
            .Where(o => o.UserId == userId && o.Purpose == purpose && o.ConsumedUtc == null)
            .OrderByDescending(o => o.Id).FirstOrDefaultAsync();

        return await ValidateCoreAsync(otp, code);
    }

    private async Task<bool> ValidateCoreAsync(OtpCode? otp, string code)
    {
        if (otp is null) return false;
        if (DateTime.UtcNow > otp.ExpiresUtc) return false;
        if (otp.Attempts >= MaxAttempts) return false;

        otp.Attempts++;

        if (otp.Code == code)
        {
            otp.ConsumedUtc = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return true;
        }

        await db.SaveChangesAsync();
        return false;
    }
}
