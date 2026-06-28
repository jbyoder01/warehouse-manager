using Domain;
using Microsoft.EntityFrameworkCore;

namespace Api;

public static class LocationEndpoints
{
    public static void MapLocationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/locations");

        // GET /locations -> List all locations
        group.MapGet("/", async (WarehouseDbContext db) => await db.Locations.ToListAsync());
        
        // GET /locations/{id} -> Get a specific location by its id
        group.MapGet("/{id:int}", async (int id, WarehouseDbContext db) =>
            await db.Locations.FindAsync(id) is Location location
                ? Results.Ok(location)
                : Results.NotFound());

        // POST /locations -> Create a new location
        group.MapPost("/", async (CreateLocationDto dto, WarehouseDbContext db) =>
        {
            var location = new Location
            {
                Name = dto.Name,
                Description = dto.Description,
            };

            db.Locations.Add(location);
            await db.SaveChangesAsync();

            return Results.Created($"/locations/{location.Id}", location);
        });

    }
}

public record CreateLocationDto(string Name, string? Description);