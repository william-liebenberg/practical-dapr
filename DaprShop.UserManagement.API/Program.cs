using DaprShop.UserManagement.API;

using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();
builder.Services.AddScoped<UserService>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
// app.UseAuthorization();
// app.MapControllers();

app.MapGet("/get", async (string username, [FromServices]UserService userService) =>
{
    User? user = await userService.GetUser(username);
    return user == null ? Results.NotFound() : Results.Ok(user);
})
.WithName("GetUser")
.WithOpenApi();

app.MapPost("/register", async (RegisterUserRequest request, [FromServices] UserService userService) =>
{
    var newUser = new User(request.Username, request.Email, request.DisplayName, request.ProfileImageUrl);
    try
    {
        await userService.AddUser(newUser);
        return Results.Ok();
    }
    catch(UserAlreadyExistsException ex) 
    {
        return Results.BadRequest(ex);
    }
})
.WithName("RegisterUser")
.WithOpenApi();

app.Run();

public record RegisterUserRequest(string Username, string Email, string DisplayName, string ProfileImageUrl)
{
}
