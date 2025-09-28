using Dealership.Api.Auth;                                              //Admin controlls the vehicles.
using Dealership.Api.Data;
using Dealership.Api.Domain;
using Dealership.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Dealership.Api.Controllers;

[ApiController]
[Route("api/admin/vehicles")]
[Authorize(Roles = Role.Admin)]
public class VehiclesAdminController(AppDbContext db, OtpService otps) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] VehicleCreateDto dto)
    {
        if (await db.Vehicles.AnyAsync(v => v.Vin == dto.Vin))
            return Conflict("VIN already exists.");

        var v = new Vehicle
        {
            Vin = dto.Vin,
            Make = dto.Make,
            Model = dto.Model,
            Year = dto.Year,
            Price = dto.Price,
            Mileage = dto.Mileage,
            BodyType = dto.BodyType,
            Color = dto.Color
        };
        db.Vehicles.Add(v);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Create), new { id = v.Id }, new { v.Id, v.Vin });
    }

    // Step 1: Start update → generate OTP for the admin
    [HttpPost("{id:int}/update/start")]
    public async Task<IActionResult> UpdateStart(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;
        if (userId is null) return Unauthorized();

        var exists = await db.Vehicles.AnyAsync(v => v.Id == id);
        if (!exists) return NotFound();

        await otps.GenerateForUserAsync(userId, OtpPurpose.UpdateVehicle);
        return Ok(new { message = "OTP sent (check server console)" });
    }

    // Step 2: Verify OTP and apply update
    [HttpPut("{id:int}/update/verify")]
    public async Task<IActionResult> UpdateVerify(int id, [FromQuery] string otp, [FromBody] VehicleUpdateDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;
        if (userId is null) return Unauthorized();

        var ok = await otps.ValidateByUserAsync(userId, OtpPurpose.UpdateVehicle, otp);
        if (!ok) return BadRequest("Invalid or expired OTP.");

        var v = await db.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
        if (v is null) return NotFound();

        // Apply only provided fields (no over-posting)
        if (dto.Make is not null) v.Make = dto.Make;
        if (dto.Model is not null) v.Model = dto.Model;
        if (dto.Year is not null) v.Year = dto.Year.Value;
        if (dto.Price is not null) v.Price = dto.Price.Value;
        if (dto.Mileage is not null) v.Mileage = dto.Mileage;
        if (dto.BodyType is not null) v.BodyType = dto.BodyType;
        if (dto.Color is not null) v.Color = dto.Color;
        if (dto.IsAvailable is not null) v.IsAvailable = dto.IsAvailable.Value;

        await db.SaveChangesAsync();
        return Ok(new { message = "Vehicle updated." });
    }
}
