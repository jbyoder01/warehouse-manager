using Domain;
using Microsoft.EntityFrameworkCore;

namespace Api;

public static class ItemEndpoints
{
    public static void MapItemEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/items");
        
        // GET /items -> List all items
        group.MapGet("/", async (WarehouseDbContext db) => await db.Items.ToListAsync());

        // GET /items/{id} -> Get a specific item by its id
        group.MapGet("/{id:int}", async (int id, WarehouseDbContext db) =>
            await db.Items.FindAsync(id) is Item item
                ? Results.Ok(item)
                : Results.NotFound());

        // POST /items -> Create a new item
        group.MapPost("/", async (CreateItemDto dto, WarehouseDbContext db) =>
        {
            var item = new Item
            {
                SKU = dto.SKU,
                Barcode = dto.Barcode,
                Name = dto.Name,
                Description = dto.Description,
            };

            db.Items.Add(item);
            await db.SaveChangesAsync();

            return Results.Created($"/items/{item.Id}", item);
        });
    }
}

// Use this instead of an Item object to control exactly what fields the client can send 
// (a client should not be able to send an id when creating, the db does that)
public record CreateItemDto(string SKU, string? Barcode, string Name, string? Description);