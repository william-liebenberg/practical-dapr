namespace DaprShop.Contracts.Events;

public class ProductItemAddedToCart
{
	public string Username { get; set; } = string.Empty;
	public string ProductId { get; set; } = string.Empty;
}