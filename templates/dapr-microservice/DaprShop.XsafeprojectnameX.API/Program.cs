using DaprShop.XsafeprojectnameX.API;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();

// TODO: Inject your services
builder.Services.AddScoped<MyService>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApiDocument(options =>
{
	options.DocumentName = "v1";
	options.Version = "v1";
	options.Title = "XsafeprojectnameX API";
	options.Description = "XsafeprojectnameX service";
});

var app = builder.Build();

app.UseCloudEvents();
app.MapSubscribeHandler();

app.UseStaticFiles(new StaticFileOptions()
{
	RequestPath = "/XsafeprojectnameX"
});

app.UseSwaggerUi3(c =>
{
	c.Path = "/XsafeprojectnameX/api";
	c.DocumentPath = "/XsafeprojectnameX/api/v1/specification.json";
});

app.MapXsafeprojectnameXEndpoints();

app.Run();