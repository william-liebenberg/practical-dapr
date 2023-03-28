namespace DaprShop.Products.API; 

public record Product(string ProductId, string Name, string Description, decimal Cost, string ImageUrl);
