using DaprShop.UserManagement.API;

using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();
builder.Services.AddScoped<UserService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Users API",
        Description = "User Management Service"
    });
});

var app = builder.Build();

app.UseCloudEvents();
app.MapSubscribeHandler();

app.UseSwagger(c =>
{
    c.RouteTemplate = "users/swagger/{documentName}/swagger.json";
});

app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("v1/swagger.json", "v1");
    o.RoutePrefix = "users/swagger";
});

app.MapUserManagementEndpoints();

app.Run();
