using DaprShop.Contracts.Entities;
using DaprShop.Contracts.Requests;
using DaprShop.Shopping.API.Services;

using Microsoft.AspNetCore.Mvc;

public static class ShoppingCartEndpoints
{
    public static IEndpointRouteBuilder MapShoppingCartRoutes(this IEndpointRouteBuilder builder)
    {
        var cartRoutes = builder.MapGroup("cart");

        cartRoutes.MapGet("get", async (string userId, [FromServices] CartService cartService) =>
        {
            try
            {
                Cart shoppingCart = await cartService.GetCart(userId);
                return Results.Ok(shoppingCart);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex);
            }
        }).WithOpenApi().WithName("GetCart"); ;

        cartRoutes.MapPost("items", async ([FromBody] AddProductItemToCart item, [FromServices] CartService cartService) =>
        {
            try
            {
                await cartService.AddItemToShoppingCart(item.UserId, item.ProductId, item.Quantity);
                return Results.Ok();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex);
            }
        }).WithOpenApi().WithName("AddItem"); ;

        cartRoutes.MapPost("submit", async ([FromBody] string userId, [FromServices] CartService cartService) =>
        {
            try
            {
                var order = await cartService.Submit(userId);
                return Results.Ok(order);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex);
            }
        }).WithOpenApi().WithName("SubmitOrder"); ;

        return builder;
    }
}