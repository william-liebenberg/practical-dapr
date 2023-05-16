using Dapr.Client;

using DaprShop.Contracts.Entities;
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

		notifications.MapPost("OrderCompleted", async (
			[FromBody] OrderCompleted orderCompletedEvent,
			[FromServices]EmailService emailService,
			[FromServices] DaprClient dapr) =>
		{
			Console.WriteLine($"Received OrderCompleted: {orderCompletedEvent.OrderId} for {orderCompletedEvent.Username}");

			// Can't call directly into the state store of other services... the key is prefixed with the owner's appid
			//Order order = await dapr.GetStateAsync<Order>(StateStoreName, orderCompletedEvent.OrderId);
			//User user = await dapr.GetStateAsync<User>(StateStoreName, orderCompletedEvent.Username);

			HttpClient productsHttpClient = DaprClient.CreateInvokeHttpClient("products-api");
			productsHttpClient.BaseAddress = new Uri("https://products-api");

			HttpClient ordersHttpClient = DaprClient.CreateInvokeHttpClient("orders-api");
			ordersHttpClient.BaseAddress = new Uri("https://orders-api");

			Order? order = await ordersHttpClient.GetFromJsonAsync<Order>($"orders/get?productId={orderCompletedEvent.OrderId}");

			HttpClient usersHttpClient = DaprClient.CreateInvokeHttpClient("users-api");
			usersHttpClient.BaseAddress = new Uri("https://users-api");

			User? user = await usersHttpClient.GetFromJsonAsync<User>($"users/get?username={orderCompletedEvent.Username}");
			
			if (order is null || user is null)
			{
				Console.WriteLine("Missing user or order details!");
				return await Task.FromResult(Results.BadRequest());
			}

			var body = $$"""
			<h1>Your order has been completed</h1>"
			<br>
			<p>Order Status: {{ order?.Status ?? OrderStatus.OrderForgotten }}</p>
			<ul>
			""";

			decimal total = 0;

			if(order is not null)
			{
				foreach (var item in order.Items)
				{
					//Product product = await dapr.GetStateAsync<Product>(StateStoreName, item.ProductId);
					Product? product = await productsHttpClient.GetFromJsonAsync<Product>($"products/get?productId={item.ProductId}");
					if (product is not null)
					{
						body += $$"""
						<li>{{item.Quantity}}x {{product.Name}} - ${{product.UnitPrice:F2}}"</li>
						""";

						total += item.Quantity * product.UnitPrice;
					}
				}
			}
			
			body += $$"""
			</ul>
			<p>Total (incl GST): ${{total:F2}}</p>
			<p>Thanks</p>
			""";
			
			Console.WriteLine($"Sending order completed email to: {user.Email}");
			var email = new EmailModel(
				From: "awliebenberg@outlook.com",
				To: user.Email,
				Subject: "Your DaprShop order has been completed!",
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
