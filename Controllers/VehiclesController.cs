using Dealership.Api.Data;                     //To control the request. 
using Dealership.Api.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;         //gives access to MVC features like ControllerBase, ActionResult, [HttpGet].
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Dealership.Api.Controllers;

[ApiController]           //marks this class as an API controller. 
[Route("api/[controller]")]
public class VehiclesController(AppDbContext db) : ControllerBase
{
    /// <summary>
    /// Browse vehicles (available by default). Supports basic filters and pagination.
    /// </summary>
    [HttpGet]    //Get req, /api/vehicles. 
    public async Task<ActionResult<IEnumerable<VehicleListItemDto>>> Browse(                          //returns a paged list of vehicles.      
        [FromQuery] string? make,
        [FromQuery] string? model,
        [FromQuery] int? minYear,
        [FromQuery] int? maxYear,
        [FromQuery] decimal? maxPrice,
        [FromQuery] bool? isAvailable,                                          ///api/vehicles?make=Toyota&page=2
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page <= 0 || pageSize <= 0) return BadRequest("page and pageSize must be positive.");

        var q = db.Vehicles.AsNoTracking().AsQueryable(); //Creates a query.

        // default to available vehicles
        q = (isAvailable is null) ? q.Where(v => v.IsAvailable) : q.Where(v => v.IsAvailable == isAvailable);   //If isAvailable is not provided, make it as True. 

        if (!string.IsNullOrWhiteSpace(make)) q = q.Where(v => v.Make  == make);
        if (!string.IsNullOrWhiteSpace(model)) q = q.Where(v => v.Model == model);
        if (minYear is not null) q = q.Where(v => v.Year >= minYear);
        if (maxYear is not null) q = q.Where(v => v.Year <= maxYear);
        if (maxPrice is not null) q = q.Where(v => v.Price <= maxPrice);

        var items = await q
            .OrderByDescending(v => v.CreatedUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)      //limits results per page.
            .Select(v => new VehicleListItemDto(v.Id, v.Vin, v.Make, v.Model, v.Year, v.Price, v.IsAvailable))
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>
    /// Get vehicle details by id.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<VehicleDetailDto>> Detail(int id)
    {
        var v = await db.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
        if (v is null) return NotFound();

        var dto = new VehicleDetailDto(v.Id, v.Vin, v.Make, v.Model, v.Year, v.Price,
            v.Mileage, v.BodyType, v.Color, v.IsAvailable, v.CreatedUtc);
        return Ok(dto);
    }
}
