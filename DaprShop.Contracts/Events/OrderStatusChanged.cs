using DaprShop.Contracts.Entities;

namespace DaprShop.Contracts.Events;

public class OrderStatusChanged
{
	public string OrderId { get; set; } = string.Empty;
	public OrderStatus? CurrentStatus { get; set; }
	public OrderStatus? PreviousStatus { get; set; }
}