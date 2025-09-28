using Dealership.Api.Auth;               //Customers see their requests. 
using Dealership.Api.Data;
using Dealership.Api.Domain;
using Dealership.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Dealership.Api.Controllers;

[ApiController]
[Route("api/purchase-requests")]
[Authorize(Roles = Role.Customer)]
public class PurchaseRequestsController(AppDbContext db, OtpService otps) : ControllerBase
{
    private string? CurrentUserId =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
        User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

    // Step 1: Start (generate OTP)
    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] PurchaseRequestStartDto dto)
    {
        var vehicle = await db.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == dto.VehicleId);
        if (vehicle is null || !vehicle.IsAvailable) return BadRequest("Vehicle not available.");

        // Block duplicate active requests for same user+vehicle
        var userId = CurrentUserId!;
        var hasActive = await db.PurchaseRequests.AnyAsync(pr =>
            pr.UserId == userId &&
            pr.VehicleId == dto.VehicleId &&
            pr.Status == PurchaseRequestStatus.Pending);
        if (hasActive) return Conflict("You already have a pending request for this vehicle.");

        await otps.GenerateForUserAsync(userId, OtpPurpose.PurchaseRequest);
        return Ok(new { message = "OTP sent (check server console)" });
    }

    // Step 2: Confirm with OTP (creates the request)
    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] PurchaseRequestConfirmDto dto)
    {
        var userId = CurrentUserId!;
        var ok = await otps.ValidateByUserAsync(userId, OtpPurpose.PurchaseRequest, dto.OtpCode);
        if (!ok) return BadRequest("Invalid or expired OTP.");

        var vehicle = await db.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == dto.VehicleId);
        if (vehicle is null || !vehicle.IsAvailable) return BadRequest("Vehicle not available.");

        var pr = new PurchaseRequest { UserId = userId, VehicleId = dto.VehicleId };
        db.PurchaseRequests.Add(pr);
        await db.SaveChangesAsync();

        return Ok(new { pr.Id, pr.Status, pr.RequestedAtUtc });
    }

    // View my requests
    [HttpGet("mine")]
    public async Task<ActionResult<IEnumerable<PurchaseRequestItemDto>>> Mine()
    {
        var userId = CurrentUserId!;
        var items = await db.PurchaseRequests.AsNoTracking()
            .Where(pr => pr.UserId == userId)
            .OrderByDescending(pr => pr.RequestedAtUtc)
            .Join(db.Vehicles, pr => pr.VehicleId, v => v.Id,
                (pr, v) => new PurchaseRequestItemDto(pr.Id, v.Id, v.Make, v.Model, v.Year, v.Price, pr.Status, pr.RequestedAtUtc))
            .ToListAsync();

        return Ok(items);
    }
}
