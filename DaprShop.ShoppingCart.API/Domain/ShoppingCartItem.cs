namespace DaprShop.ShoppingCart.API.Domain;

public record ShoppingCartItem(string? ProductId, string? ProductName, decimal Price, int Quantity);