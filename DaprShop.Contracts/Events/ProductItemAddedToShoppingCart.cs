namespace DaprShop.Contracts.Events;

public class ProductItemAddedToShoppingCart
{
    public string UserId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
}