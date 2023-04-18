namespace DaprShop.Contracts.Requests;

public record AddProductItemToCart(string Username, string ProductId, int Quantity);