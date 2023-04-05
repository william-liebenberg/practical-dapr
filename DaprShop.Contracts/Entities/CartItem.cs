namespace DaprShop.Contracts.Entities;

public record CartItem(string? ProductId, string? ProductName, decimal Price, int Quantity);
