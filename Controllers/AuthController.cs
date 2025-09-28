using Dealership.Api.Auth;
using Dealership.Api.Data;
using Dealership.Api.Domain;
using Dealership.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dealership.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext db, PasswordHasher hasher, JwtTokenService jwt, OtpService otps, ILogger<AuthController> log)
    : ControllerBase
{
    /// Start Register: create OTP for email (no user yet)
    [HttpPost("register/start")]
    public async Task<IActionResult> RegisterStart([FromBody] RegisterStartDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Email and password are required.");

        var exists = await db.Users.AnyAsync(u => u.Email == dto.Email);
        if (exists) return Conflict("Email already registered.");

        // Persist the password temporarily? No—avoid storing raw pwd.
        // We regenerate on verify, using the same input from client.
        await otps.GenerateForEmailAsync(dto.Email, OtpPurpose.Register);
        return Ok(new { message = "OTP sent (check server console)" });
    }

    /// Verify Register OTP: create user with hashed password
    [HttpPost("register/verify")]
    public async Task<ActionResult<AuthResultDto>> RegisterVerify([FromBody] RegisterVerifyDto dto, [FromQuery] string password)
    {
        // password passed again via query (or body in real apps) to avoid temporary storage
        if (!await otps.ValidateByEmailAsync(dto.Email, OtpPurpose.Register, dto.OtpCode))
            return BadRequest("Invalid or expired OTP.");

        var user = new User
        {
            Email = dto.Email,
            PasswordHash = hasher.Hash(password),
            Role = Role.Customer
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var token = jwt.Create(user);
        return Ok(new AuthResultDto(token, user.Role, user.Email));
    }

    /// Start Login: check password, then create OTP for user
    [HttpPost("login/start")]
    public async Task<IActionResult> LoginStart([FromBody] LoginStartDto dto)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user is null) return Unauthorized("Invalid credentials.");

        if (!hasher.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials.");

        await otps.GenerateForUserAsync(user.Id, OtpPurpose.Login);
        return Ok(new { message = "OTP sent (check server console)" });
    }

    /// Verify Login OTP → issue JWT
    [HttpPost("login/verify")]
    public async Task<ActionResult<AuthResultDto>> LoginVerify([FromBody] LoginVerifyDto dto)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user is null) return Unauthorized("Invalid credentials.");

        var ok = await otps.ValidateByUserAsync(user.Id, OtpPurpose.Login, dto.OtpCode);
        if (!ok) return BadRequest("Invalid or expired OTP.");

        var token = jwt.Create(user);
        return Ok(new AuthResultDto(token, user.Role, user.Email));
    }
}
