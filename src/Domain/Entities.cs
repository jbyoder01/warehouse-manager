namespace Domain;

public class Item
{
    public int Id { get; set; }
    public required string SKU { get; set; }
    public string? Barcode { get; set; } // Should match the manufacturer's barcode.
    public required string Name { get; set; } 
    public string? Description { get; set; }
}

public class Location
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}

public class Transaction             // append-only ledger of every change
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public int LocationId { get; set; }
    public int QuantityChange { get; set; } // Positiv = in, negative = out
    public TransactionType Type { get; set; } 
    public Guid? MoveId { get; set; } // Links the add/remove operations performed during a move
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public required string UserId { get; set; }        // who made the change (Entra id later)
}

public enum TransactionType
{
    Add,
    Remove,
    Move,
}
