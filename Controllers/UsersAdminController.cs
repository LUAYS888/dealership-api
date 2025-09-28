using Dealership.Api.Data;                 //Admin see customers. 
using Dealership.Api.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dealership.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = Role.Admin)]
public class UsersAdminController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListCustomers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page <= 0 || pageSize <= 0) return BadRequest("Invalid paging.");

        var users = await db.Users.AsNoTracking()
            .Where(u => u.Role == Role.Customer)
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new { u.Id, u.Email, u.CreatedUtc })
            .ToListAsync();

        return Ok(users);
    }
}
