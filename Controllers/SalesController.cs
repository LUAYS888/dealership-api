using Dealership.Api.Data;                        //Admin process the sale.   Also customers see the purchase history. 
using Dealership.Api.Domain;
using Dealership.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Dealership.Api.Controllers;

[ApiController]
[Route("api/sales")]
public class SalesController(AppDbContext db) : ControllerBase
{
    // Admin: process a sale
    [Authorize(Roles = Role.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaleCreateDto dto)
    {
        var vehicle = await db.Vehicles.FirstOrDefaultAsync(v => v.Id == dto.VehicleId);
        if (vehicle is null) return NotFound("Vehicle not found.");
        if (!vehicle.IsAvailable) return Conflict("Vehicle already sold/unavailable.");

        var customer = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.CustomerEmail);
        if (customer is null) return NotFound("Customer not found.");

        // optional: mark any pending PRs as converted
        var pending = await db.PurchaseRequests
            .Where(pr => pr.UserId == customer.Id && pr.VehicleId == dto.VehicleId && pr.Status == PurchaseRequestStatus.Pending)
            .ToListAsync();
        foreach (var pr in pending) pr.Status = PurchaseRequestStatus.ConvertedToSale;

        vehicle.IsAvailable = false;

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value!;

        var sale = new Sale
        {
            VehicleId = vehicle.Id,
            CustomerId = customer.Id,
            FinalPrice = dto.FinalPrice,
            ProcessedByAdminId = adminId
        };

        db.Sales.Add(sale);
        await db.SaveChangesAsync();

        return Ok(new { sale.Id, sale.VehicleId, sale.FinalPrice, sale.SoldAtUtc });
    }

    // Customer: view my purchase history
    [Authorize(Roles = Role.Customer)]
    [HttpGet("mine")]
    public async Task<ActionResult<IEnumerable<SaleItemDto>>> Mine()
    {
        var userId =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value!;

        var items = await db.Sales.AsNoTracking()
            .Where(s => s.CustomerId == userId)
            .OrderByDescending(s => s.SoldAtUtc)
            .Select(s => new SaleItemDto(s.Id, s.VehicleId, s.FinalPrice, s.SoldAtUtc, s.IsVoided))
            .ToListAsync();

        return Ok(items);
    }
}
