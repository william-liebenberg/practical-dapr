namespace DaprShop.ShoppingCart.API.Domain;

public class ShoppingCartItem
{
    public string? ProductId {get;set;} = string.Empty;
    public string? ProductName {get;set;} = string.Empty;
    public decimal Price {get;set;} 
    public int Quantity {get;set;} 
}
