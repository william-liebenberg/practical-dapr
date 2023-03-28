namespace DaprShop.ShoppingCart.API.Domain;

public enum OrderStatus
{
    OrderNew,
    OrderReceived,
    OrderProcessing,
    OrderComplete
}

// TODO: SHould this be in Contracts?
public record Product(string ProductId, string Name, string Description, decimal Cost, string ImageUrl);

// LineItem is a simple line on an order, a tuple of count and a Product struct
public record OrderItem(int Quantity, Product Product);

// Order holds information about a customer order
public record Order(string OrderId, string CustomerId, string Title, decimal TotalAmount, OrderItem[] Items, OrderStatus Status);