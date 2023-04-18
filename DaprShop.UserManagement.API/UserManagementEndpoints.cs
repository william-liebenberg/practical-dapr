using DaprShop.Contracts.Entities;
using DaprShop.UserManagement.API;

using Microsoft.AspNetCore.Mvc;

public static class UserManagementEndpoints
{
	public static void MapUserManagementEndpoints(this IEndpointRouteBuilder builder)
	{
		var userRoutes = builder
			//.WithOpenApi()
			.MapGroup("users")
			.WithTags(new[] { "Users" });

		userRoutes.MapGet("get", async (string username, [FromServices] UserService userService) =>
		{
			User? user = await userService.GetUser(username);
			return user == null ? Results.NotFound() : Results.Ok(user);
		})
			//.WithOpenApi()
			.WithName("GetUser");

		userRoutes.MapGet("isRegistered", async (string username, [FromServices] UserService userService) =>
		{
			User? user = await userService.GetUser(username);
			return user == null ? Results.NotFound() : Results.Ok();
		})
			//.WithOpenApi()
			.WithName("IsRegistered");

		userRoutes.MapPost("register", async (RegisterUserRequest request, [FromServices] UserService userService) =>
		{
			var newUser = new User(request.Username, request.Email, request.DisplayName, request.ProfileImageUrl);
			try
			{
				await userService.AddUser(newUser);
				return Results.Ok();
			}
			catch (UserAlreadyExistsException ex)
			{
				return Results.BadRequest(ex);
			}
		})
			//.WithOpenApi()
			.WithName("RegisterUser");
	}
}