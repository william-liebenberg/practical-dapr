using DaprShop.Contracts.Events;

using Microsoft.AspNetCore.Mvc;

namespace DaprShop.Notifications.API;

public static class NotificationsEndpoints
{
	private static readonly string PubSubName = "daprshop-pubsub";
	private static readonly string OrderCompletedTopic = "daprshop.orders.completed";

	public static void MapNotificationsEndpoints(this IEndpointRouteBuilder builder)
	{
		RouteGroupBuilder notifications = builder
			.MapGroup("notifications")
			.WithTags(new[] { "Notifications" });

		notifications.MapPost("OrderCompleted", async ([FromBody] OrderCompleted orderCompletedEvent) =>
		{
			Console.WriteLine($"Received OrderCompleted: {orderCompletedEvent.OrderId} for {orderCompletedEvent.Username}");
			return await Task.FromResult(Results.Ok());
		})
			.WithTopic(PubSubName, OrderCompletedTopic)
			.WithName("ReceiveCompletedOrder");
	}
}
