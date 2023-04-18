namespace DaprShop.Contracts.Entities;

public record Product(string ProductId, string Name, string Description, decimal UnitPrice, string ImageUrl);