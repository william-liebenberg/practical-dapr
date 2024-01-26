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
			.WithTags(new[] { "Cart" });

		cartRoutes.MapGet("healthz", () => { return Results.Ok(); });

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
		.WithName("GetCart");

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
		.WithName("AddItem");

		cartRoutes.MapPost("submit", async ([FromBody] SubmitCartRequest req, [FromServices] CartService cartService) =>
		{
			try
			{
				Order? order = await cartService.SubmitNewOrder(req.Username);
				return order is null ? Results.BadRequest() : Results.Ok(order);
			}
			catch (Exception ex)
			{
				return Results.BadRequest(ex);
			}
		})
		.WithName("SubmitOrder");

		return builder;
	}
}