using DaprShop.Contracts.Events;

using Microsoft.AspNetCore.Mvc;

namespace DaprShop.Notifications.API;

public static class NotificationsEndpoints
{
	private static readonly string PubSubName = "daprshop-pubsub";
	private static readonly string OrderCompletedTopic = "daprshop.orders.completed";
	private static readonly string CartTopic = "daprshop.cart.items";

	public static void MapNotificationsEndpoints(this IEndpointRouteBuilder builder)
	{
		RouteGroupBuilder notifications = builder
			.MapGroup("notifications")
			.WithTags(new[] { "Notifications" });

		// notifications.MapGet("healthz", () => { return Results.Ok(); });

		notifications.MapPost("OrderCompleted", async ([FromBody] OrderCompleted orderCompletedEvent) =>
		{
			Console.WriteLine($"Received OrderCompleted: {orderCompletedEvent.OrderId} for {orderCompletedEvent.Username}");
			return await Task.FromResult(Results.Ok());
		})
			.WithTopic(PubSubName, OrderCompletedTopic)
			.WithName("ReceiveCompletedOrder");


		notifications.MapPost("added", ([FromBody] ProductItemAddedToCart @event) =>
		{
			Console.WriteLine($"---===> Item [{@event.ProductId}] added to [{@event.Username}]'s cart.");
			return Results.Ok();
		})
			.WithTopic(PubSubName, CartTopic)
			.WithName("ItemAdded")
			.ExcludeFromDescription();

		notifications.MapPost("removed", ([FromBody] ProductItemRemovedFromCart @event) =>
		{
			Console.WriteLine($"---===> Item [{@event.ProductId}] removed from [{@event.Username}]'s cart.");
			return Results.Ok();
		})
			.WithTopic(PubSubName, CartTopic)
			.WithName("ItemRemoved")
			.ExcludeFromDescription();

		notifications.MapPost("cleared", ([FromBody] CartCleared @event) =>
		{
			Console.WriteLine($"---===> Cleared cart for [{@event.Username}]");
			return Results.Ok();
		})
			.WithTopic(PubSubName, CartTopic)
			.WithName("Cleared")
			.ExcludeFromDescription();

		notifications.MapPost("StatusChanged", ([FromBody] OrderStatusChanged @event) =>
		{
			Console.WriteLine($"---===> Order [{@event.OrderId}] status changed from [{@event.PreviousStatus}] to [{@event.CurrentStatus}]");
			return Results.Ok();
		})
			.WithTopic(PubSubName, CartTopic)
			.WithName("StatusChanged")
			.ExcludeFromDescription();
	}
}