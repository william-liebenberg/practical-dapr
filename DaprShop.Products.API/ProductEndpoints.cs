namespace DaprShop.Products.API;

using Microsoft.AspNetCore.Mvc;

public static class ProductEndpoints
{
	public static void MapProductEndpoints(this IEndpointRouteBuilder builder)
	{
		var endpoints = builder
			.MapGroup("products")
			.WithTags(new[] { "Products" });


		endpoints.MapGet("get", async (string productId, [FromServices] ProductService productService) =>
		{
			var result = await productService.GetProduct(productId);
			return result == null ? Results.NotFound() : Results.Ok(result);
		}).WithName("GetProduct");

		endpoints.MapGet("catalogue", async ([FromServices] ProductService productService) =>
		{
			var results = await productService.ListAll();
			return Results.Ok(results);
		}).WithName("GetCatalogue");

		//endpoints.MapGet("search", async (string field, string searchTerm, [FromServices] ProductService productService) =>
		//{
		//	var results = await productService.Search(field, searchTerm);
		//	return Results.Ok(results);
		//})
		//	//.WithOpenApi()
		//	.WithName("Search");

		//endpoints.MapGet("query", async (string searchTerm, [FromServices] ProductService productService) =>
		//{
		//	var results = await productService.Query(searchTerm);
		//	return Results.Ok(results);
		//})
		//	//.WithOpenApi()
		//	.WithName("Query");

		endpoints.MapPost("seed", async ([FromServices] ProductService productService) =>
		{
			await productService.Seed();
			return Results.Ok();
		}).WithName("Seed");
	}
}