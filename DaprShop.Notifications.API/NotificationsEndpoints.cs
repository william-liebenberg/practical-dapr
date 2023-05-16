using Dapr.Client;

using DaprShop.Contracts.Entities;
using DaprShop.Contracts.Events;

using Microsoft.AspNetCore.Mvc;

namespace DaprShop.Notifications.API;

public static class NotificationsEndpoints
{
	private static readonly string StateStoreName = "daprshop-statestore";
	private static readonly string PubSubName = "daprshop-pubsub";
	private static readonly string OrderCompletedTopic = "daprshop.orders.completed";

	public static void MapNotificationsEndpoints(this IEndpointRouteBuilder builder)
	{
		RouteGroupBuilder notifications = builder
			.MapGroup("notifications")
			.WithTags(new[] { "Notifications" });

		notifications.MapPost("OrderCompleted", async (
			[FromBody] OrderCompleted orderCompletedEvent,
			[FromServices]EmailService emailService,
			[FromServices] DaprClient dapr) =>
		{
			Console.WriteLine($"Received OrderCompleted: {orderCompletedEvent.OrderId} for {orderCompletedEvent.Username}");

			Order order = await dapr.GetStateAsync<Order>(StateStoreName, orderCompletedEvent.OrderId);
			User user = await dapr.GetStateAsync<User>(StateStoreName, orderCompletedEvent.Username);

			var body = $$"""
			<h1>Your order has been completed</h1>"
			<br>
			<p>Order Status: {{ order.Status}}</p>
			<ul>
			""";

			decimal total = 0;
			foreach(var item in order.Items)
			{
				Product product = await dapr.GetStateAsync<Product>(StateStoreName, item.ProductId);

				body += $$"""
				<li>{{item.Quantity}}x {{product.Name}} - ${{product.UnitPrice:F2}}"</li>
				""";

				total += item.Quantity * product.UnitPrice;
			}

			body += $$"""
			</ul>
			<p>Total (incl GST): ${{total:F2}}</p>
			<p>Thanks</p>
			""";

			var email = new EmailModel(
				From: "awliebenberg@outlook.com",
				To: user.Email,
				Subject: "Your order has been completed",
				CC: "awliebenberg@outlook.com",
				BCC: null!,
				Body: body);

			await emailService.SendEmail(email);

			return await Task.FromResult(Results.Ok());
		})
			.WithTopic(PubSubName, OrderCompletedTopic)
			.WithName("ReceiveCompletedOrder");
	}
}
