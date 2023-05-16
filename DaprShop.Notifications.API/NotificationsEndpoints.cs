using DaprShop.Contracts.Events;

using Microsoft.AspNetCore.Mvc;

namespace DaprShop.Notifications.API;

public static class NotificationsEndpoints
{
	private static readonly string OrderCompletedTopic = "daprshop.orders.completed";

	public static void MapNotificationsEndpoints(this IEndpointRouteBuilder builder)
	{
		RouteGroupBuilder notifications = builder
			.MapGroup("notifications")
			.WithTags(new[] { "Notifications" });

		notifications.MapPost("submit", async ([FromBody] OrderCompletedEvent orderCompletedEvent) =>
		{
			Console.WriteLine("Received OrderCompleted: {orderId} for {username}", orderCompletedEvent.OrderId, orderCompletedEvent.Username);
			return await Task.FromResult(Results.Ok());
		})
			.WithTopic("daprshop-pubsub", OrderCompletedTopic)
			.WithName("ReceiveCompletedOrder");
	}
}
