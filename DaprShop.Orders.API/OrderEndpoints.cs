using DaprShop.Contracts.Entities;
using DaprShop.Orders.API;

using Microsoft.AspNetCore.Mvc;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder orders = builder
            .MapGroup("orders")
            .WithTags(new []{"Orders"});
				//.WithOpenApi();

        orders.MapGet("get", async (string orderId, [FromServices] OrderService orderService) =>
        {
            var result = await orderService.GetOrder(orderId);
            return Results.Ok(result);
        })
            //.WithOpenApi()
            .WithName("GetOrder");

        orders.MapGet("user", async (string username, [FromServices] OrderService orderService) =>
        {
            var result = await orderService.GetOrdersForUser(username);
            return Results.Ok(result);
        })            
            //.WithOpenApi()
            .WithName("GetOrdersForUser");

        orders.MapPost("submit", async ([FromBody] Order order, [FromServices] OrderService orderService) =>
        {
            await orderService.ProcessOrder(order);
            return Results.Ok();
        })
            .WithTopic("daprshop-pubsub", "daprshop.orders.queue")
            //.WithOpenApi()
            .WithName("ReceiveOrder");
    }
}