using DaprShop.Contracts.Entities;
using DaprShop.Contracts.Events;
using DaprShop.Contracts.Requests;
using DaprShop.Shopping.API.Services;

using Microsoft.AspNetCore.Mvc;

public static class ShoppingCartEndpoints
{
    public static IEndpointRouteBuilder MapShoppingCartRoutes(this IEndpointRouteBuilder builder)
    {
        var cartRoutes = builder
            .MapGroup("cart")
            .WithTags(new []{"Cart"})
            .WithOpenApi();

        cartRoutes.MapGet("get", async (string username, [FromServices] CartService cartService) =>
        {
            try
            {
                Cart shoppingCart = await cartService.GetCart(username);
                return Results.Ok(shoppingCart);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex);
            }
        })
            .WithOpenApi()
            .WithName("GetCart"); ;

        cartRoutes.MapPost("items", async ([FromBody] AddProductItemToCart item, [FromServices] CartService cartService) =>
        {
            try
            {
                await cartService.AddItemToShoppingCart(item.Username, item.ProductId, item.Quantity);
                return Results.Ok();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex);
            }
        })
            .WithOpenApi()
            .WithName("AddItem"); ;

        cartRoutes.MapPost("submit", async ([FromBody] SubmitCartRequest req, [FromServices] CartService cartService) =>
        {
            try
            {
                Order? order = await cartService.Submit(req.Username);
                return order is null ? Results.BadRequest() : Results.Ok(order);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex);
            }
        })
            .WithOpenApi()
            .WithName("SubmitOrder");




        string pubsubName = "daprshop-pubsub";
        string cartTopic = "daprshop.cart.items";

        cartRoutes.MapPost("added", ([FromBody] ProductItemAddedToCart @event) =>
        {
            Console.WriteLine($"---===> Item [{@event.ProductId}] added to [{@event.Username}]'s cart.");
            return Results.Ok();
        })
            .WithTopic(pubsubName, cartTopic)
            .WithOpenApi()
            .WithName("ItemAdded");

        cartRoutes.MapPost("removed", ([FromBody] ProductItemRemovedFromCart @event) =>
        {
            Console.WriteLine($"---===> Item [{@event.ProductId}] removed from [{@event.Username}]'s cart.");
            return Results.Ok();
        })
            .WithTopic(pubsubName, cartTopic)
            .WithOpenApi()
            .WithName("ItemRemoved");

        cartRoutes.MapPost("cleared", ([FromBody] CartCleared @event) =>
        {
            Console.WriteLine($"---===> Cleared cart for [{@event.Username}]");
            return Results.Ok();
        })
            .WithTopic(pubsubName, cartTopic)
            .WithOpenApi()
            .WithName("Cleared");

        cartRoutes.MapPost("StatusChanged", ([FromBody] OrderStatusChanged @event) =>
        {
            Console.WriteLine($"---===> Order [{@event.OrderId}] statuc changed from [{@event.PreviousStatus}] to [{@event.CurrentStatus}]");
            return Results.Ok();
        })
            .WithTopic(pubsubName, cartTopic)
            .WithOpenApi()
            .WithName("StatusChanged");
        
        return builder;
    }
}

