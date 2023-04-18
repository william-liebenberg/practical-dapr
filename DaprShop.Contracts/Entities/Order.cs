namespace DaprShop.Contracts.Entities;

public enum OrderStatus
{
	OrderNew,
	OrderReceived,
	OrderProcessing,
	OrderComplete
}

// LineItem is a simple line on an order, a tuple of count and a Product struct
public record OrderItem(int Quantity, string ProductId);

// Order holds information about a user order
public record Order(string OrderId, string Username, string Title, decimal TotalAmount, OrderItem[] Items, OrderStatus Status);