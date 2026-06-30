using Domain;
using Microsoft.EntityFrameworkCore;

namespace Api;

public static class TransactionEndpoints
{
    public static void MapTransactionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/transactions");

        // GET /transactions/quantity?itemId=1&locationId=2
        // current quantity of an item at a location, computed from the log.
        group.MapGet("/quantity", async (int itemId, int locationId, WarehouseDbContext db) =>
        {
            var qty = await db.Transactions
                .Where(t => t.ItemId == itemId && t.LocationId == locationId)
                .SumAsync(t => t.QuantityChange);

            return Results.Ok(new { itemId, locationId, quantity = qty });
        });

        // GET /transactions/item/{itemId}
        // all locations where an item is (must include the check for non zero locations since we're summing the ledger)
        group.MapGet("/item/{itemId:int}", async (int itemId, WarehouseDbContext db) =>
        {
            var byLocation = await db.Transactions
                .Where(t => t.ItemId == itemId)
                .GroupBy(t => t.LocationId)
                .Select(g => new { locationId = g.Key, quantity = g.Sum(t => t.QuantityChange) })
                .Where(x => x.quantity != 0)
                .ToListAsync();
            
            return Results.Ok(byLocation);
        });

        // POST /transaction/add
        // stock arrives
        group.MapPost("/add", async (AddRemoveDto dto, WarehouseDbContext db) =>
        {
            if (dto.Quantity <= 0)
            {
                return Results.BadRequest("Quantity must be positive.");
            }

            db.Transactions.Add(new Transaction
            {
                ItemId = dto.ItemId,
                LocationId = dto.LocationId,
                QuantityChange = dto.Quantity,
                Type = TransactionType.Add,
                UserId = "dev-user",
            });

            await db.SaveChangesAsync();
            return Results.Ok();
        });
        
        // POST /transactions/remove
        // stock leaves
        group.MapPost("/remove", async (AddRemoveDto dto, WarehouseDbContext db) =>
        {
            if (dto.Quantity <= 0)
            {
                return Results.BadRequest("Quantity must be positive.");
            }

            var onHand = await db.Transactions
                .Where(t => t.ItemId == dto.ItemId && t.LocationId == dto.LocationId)
                .SumAsync(t => t.QuantityChange);

            if (onHand < dto.Quantity)
            {
                return Results.BadRequest($"Not enough stock. On hand: {onHand} Requested: {dto.Quantity}");
            }

            db.Transactions.Add(new Transaction
            {
                ItemId = dto.ItemId,
                LocationId = dto.LocationId,
                QuantityChange = -dto.Quantity,
                Type = TransactionType.Remove,
                UserId = "dev-user",
            });

            await db.SaveChangesAsync();
            return Results.Ok();
        });

        // POST/transactions/move
        // move stock in the warehouse
        group.MapPost("/move", async (MoveDto dto, WarehouseDbContext db) =>
        {
            if (dto.Quantity <= 0)
            {
                return Results.BadRequest("Quantity must be positive.");
            }

            if (dto.FromLocationId == dto.ToLocationId)
            {
                return Results.BadRequest("Source and destination must differ.");
            }

            var onHand = await db.Transactions
                .Where(t => t.ItemId == dto.ItemId && t.LocationId == dto.FromLocationId)
                .SumAsync(t => t.QuantityChange);
            if (onHand < dto.Quantity)
            {
                return Results.BadRequest($"Not enough stock. On hand: {onHand} Requested: {dto.Quantity}");
            }

            var moveId = Guid.NewGuid();

            await using var tx = await db.Database.BeginTransactionAsync();
            db.Transactions.Add(new Transaction
            {
                ItemId = dto.ItemId,
                LocationId = dto.FromLocationId,
                QuantityChange = -dto.Quantity,
                Type = TransactionType.Move,
                MoveId = moveId,
                UserId = "dev-user",
            });

            db.Transactions.Add(new Transaction
            {
                ItemId = dto.ItemId,
                LocationId = dto.ToLocationId,
                QuantityChange = dto.Quantity,
                Type = TransactionType.Move,
                MoveId = moveId,
                UserId = "dev-user",
            });

            await db.SaveChangesAsync();
            await tx.CommitAsync();

            return Results.Ok(new { moveId });
        });
    }
}

public record AddRemoveDto(int ItemId, int LocationId, int Quantity);
public record MoveDto(int ItemId, int FromLocationId, int ToLocationId, int Quantity);