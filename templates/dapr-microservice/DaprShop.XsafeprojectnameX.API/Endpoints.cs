namespace DaprShop.XsafeprojectnameX.API;

using Microsoft.AspNetCore.Mvc;

public static class Endpoints
{
	public static void MapXsafeprojectnameXEndpoints(this IEndpointRouteBuilder builder)
	{
		var serviceRoutes = builder
			//.WithOpenApi()
			.MapGroup("services")
			.WithTags(new[] { "Services" });

		serviceRoutes.MapGet("get", async ([FromQuery]string message, [FromServices] MyService myService) =>
		{
			var response = await myService.Execute(message);
			return response == null ? Results.NotFound() : Results.Ok(response);
		})
			//.WithOpenApi()
			.WithName("GetResponse");
	}
}