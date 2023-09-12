using Dapr;

using DaprShop.Contracts.Entities;
using DaprShop.Orders.API;

using Microsoft.AspNetCore.Mvc;

public static class OrderEndpoints
{
	public static void MapOrderEndpoints(this IEndpointRouteBuilder builder)
	{
		RouteGroupBuilder orders = builder
			.MapGroup("orders")
			.WithTags(new[] { "Orders" });

		orders.MapGet("get", async (string orderId, [FromServices] OrderService orderService) =>
		{
			var result = await orderService.GetOrder(orderId);
			return Results.Ok(result);
		}).WithName("GetOrder");

		orders.MapGet("user", async (string username, [FromServices] OrderService orderService) =>
		{
			var result = await orderService.GetOrdersForUser(username);
			return Results.Ok(result);
		}).WithName("GetOrdersForUser");

		//orders.MapPost("submit", async ([FromBody] Order order, [FromServices] OrderService orderService, CancellationToken cancellationToken) =>
		//{
		//	await orderService.ProcessOrder(order, cancellationToken);
		//	return Results.Ok();
		//}).WithTopic("daprshop-pubsub", "daprshop.orders.queue")
		//.WithName("ReceiveOrder");

		orders.MapPost("submit",
			[Topic("daprshop-pubsub", "daprshop.orders.queue")]
			async ([FromBody] Order order, [FromServices] OrderService orderService, CancellationToken cancellationToken) =>
		{
			await orderService.ProcessOrder(order, cancellationToken);
			return Results.Ok();
		}).WithName("ReceiveOrder");
	}
}