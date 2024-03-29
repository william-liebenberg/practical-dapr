﻿using DaprShop.Contracts.Entities;

using Microsoft.AspNetCore.Mvc;

namespace DaprShop.UserManagement.API;

public static class UserManagementEndpoints
{
	public static void MapUserManagementEndpoints(this IEndpointRouteBuilder builder)
	{
		var userRoutes = builder
			.MapGroup("users")
			.WithTags(new[] { "Users" });

		// userRoutes.MapGet("healthz", () => { return Results.Ok(); });

		userRoutes.MapGet("get", async (string username, [FromServices] UserService userService) =>
		{
			User? user = await userService.GetUser(username);
			return user == null ? Results.NotFound() : Results.Ok(user);
		}).WithName("GetUser");

		userRoutes.MapGet("isRegistered", async (string username, [FromServices] UserService userService) =>
		{
			User? user = await userService.GetUser(username);
			return user == null ? Results.NotFound() : Results.Ok();
		}).WithName("IsRegistered");

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
		}).WithName("RegisterUser");
	}
}