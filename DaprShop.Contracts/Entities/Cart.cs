namespace DaprShop.Contracts.Entities;

public class Cart
{
    public Cart() => Items = new List<CartItem>();

    public string UserId { get; set; } = string.Empty;
    public List<CartItem> Items { get; set; }
}
