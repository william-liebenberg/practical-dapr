namespace DaprShop.Contracts;

public class ProductItemAddedToShoppingCartEvent
{
    public string UserId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
}