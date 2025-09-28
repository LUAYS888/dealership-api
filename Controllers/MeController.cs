using System.IdentityModel.Tokens.Jwt;                  //For testing.  “who am I” endpoint that lets an authenticated user fetch information about themselves from their JWT claims.
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dealership.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MeController : ControllerBase
{
    [Authorize]
    [HttpGet]
    public IActionResult Get()
    {
        string? sub =
            User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        string? email =
            User.FindFirst(JwtRegisteredClaimNames.Email)?.Value ??
            User.FindFirst(ClaimTypes.Email)?.Value;

        string? role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new { userId = sub, email, role });
    }
}
