using DaprShop.UserManagement.API;

using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();
builder.Services.AddScoped<UserService>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var MyAllowSpecificOrigins = "_MyAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      builder =>
                      {
                          builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                      });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger(c =>
{
    c.RouteTemplate = "users/swagger/{documentName}/swagger.json";
    c.PreSerializeFilters.Add((swaggerDoc, httpRequest) =>
    {
        if (!httpRequest.Headers.ContainsKey("X-Forwarded-Host")) return;
        var basePath = "";
        var serverUrl = $"{httpRequest.Scheme}://{httpRequest.Headers["X-Forwarded-Host"]}/{basePath}";
        swaggerDoc.Servers = new List<OpenApiServer> { new OpenApiServer { Url = serverUrl } };
    });
});
app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("v1/swagger.json", "v1");
    o.RoutePrefix = "users/swagger";
});

app.UseCors(MyAllowSpecificOrigins);
app.UseForwardedHeaders();


app.MapGet("users/get", async (string username, [FromServices]UserService userService) =>
{
    User? user = await userService.GetUser(username);
    return user == null ? Results.NotFound() : Results.Ok(user);
})
.WithName("GetUser")
.WithOpenApi();

app.MapGet("users/isRegistered", async (string username, [FromServices] UserService userService) =>
{
    User? user = await userService.GetUser(username);
    return user == null ? Results.NotFound() : Results.Ok();
})
.WithName("IsRegistered")
.WithOpenApi();

app.MapPost("users/register", async (RegisterUserRequest request, [FromServices] UserService userService) =>
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
