# Practical Microservice Development with Dapr

![Alt](https://repobeats.axiom.co/api/embed/72c3f8104ddc59671efd2949452000a9255908d3.svg "Repobeats analytics image")

## Install Dapr CLI

```ps1
winget install Dapr.CLI
```

## Running Dapr applications

- [ ] TODO: Show how to run and debug multiple dapr applications

```ps1
dapr run --app-port 7108 --app-ssl --components-path ./components/ --app-id users-api -- dotnet run -lp https --project ./DaprShop.UserManagement.API/DaprShop.UserManagement.API.csproj
```

## Install Dapr VSCode Extension

Install the [Dapr for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-dapr) extension.

Check out the [docs](https://docs.dapr.io/developing-applications/local-development/ides/vscode/vscode-dapr-extension/) for Dapr for Visual Studio Code extension

Once you have your Dapr applications running with `dapr run`, check out the Dapr dashboard:
- http://localhost:8000/overview

- [ ] TODO: Explain how to debug Dapr applications

## Sample Application

### Initialize Dapr

Initialize Dapr at the project root:

```ps1
dapr init
```

> NOTE: You need docker (or alternative) installed and running locally for Dapr to initialize and run

### Create new .NET WebAPI project

Create a new .NET Web API project:

```sh
# Create a new WebAPI
dotnet new webapi -n DaprShop.ShoppingCart.API
cd DaprShop.ShoppingCart.API
```

### Add Dapr Packages

Add the required Dapr NuGet packages:

```sh
# Add Dapr packages for ASP.NET Core
dotnet add package Dapr.Client
dotnet add package Dapr.AspNetCore
```

### Configure Dapr Client and Middleware in .NET WebAPI

Add Dapr configuration code in `Program.cs`:

```cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();

//...

var app = builder.Build();

app.UseCloudEvents();
app.MapSubscribeHandler();

// ...

app.Run();
```

### Using Dapr Client

- [ ] TODO

### Listening for PubSub events

- [ ] TODO

## DevOps

- [ ] TODO: Complete devops story

1. Deploy Bicep to create Azure Container App Environment, ACA Applications, Dapr Components, supporting infrastructure for state stores and pubsub
2. Build apps using Docker
3. Push to ACR
4. Create revision on Azure Container Apps

## YARP as API Gateway

[YARP: Yet Another Reverse Proxy](https://microsoft.github.io/reverse-proxy/)

**Dapr** conveniently provide a [Service Invocation](https://docs.dapr.io/developing-applications/building-blocks/service-invocation/service-invocation-overview/) building block with built-in **Service Discovery**. This means we can avoid needing to know exact host names for the services we wish to invoke. Instead, we can identify services via a `Dapr App Id`.

**YARP** can match incoming requests on service specific routes (e.g `/cart`, `/orders`, `/products`) and forward the request to the appropriate backend service via the Dapr Sidecars.

In the YARP configuration, for each service specific matched route add the `dapr-app-id` request header with a value of the universal `App Id`.

Check out the Dapr [docs](https://docs.dapr.io/developing-applications/building-blocks/service-invocation/howto-invoke-discover-services/) for more details about Service Discovery.

```json
"DaprReverseProxy": {
  "Routes": {
    "users-api": {
      "ClusterId": "daprSidecar",
      // Match service specific route prefixes 
      "Match": {
        "Path": "/users/{*any}"
      },
      // add the dapr-app-id header and assign the appropriate app-id value 
      "Transforms": [{
          "RequestHeader": "dapr-app-id",
          "Append": "users-api"
      }]
    },
    //
    // OTHER ROUTES
    //
  },
  "Clusters": {
    "daprSidecar": {
      "Destinations": {
        "sidecar": {
          // forward all requests to a known sidecar - dapr will do the rest and forward to the appropriate sidecar for the target application as long as the `dapr-app-id` value is set correctly
          "Address": "http://localhost:3500/"
        }
      }
    }
  }
}
```

## API Documentation with OpenAPI/Swagger

Using [NSwag](https://github.com/RicoSuter/NSwag) (specifically [NSwag.AspNetCore](https://www.nuget.org/packages/NSwag.AspNetCore) and [NSwag.MsBuild](https://github.com/RicoSuter/NSwag/wiki/NSwag.MSBuild) NuGet packages)  we can generate OpenAPI 3.x or Swagger 2.x API Specification documents for an ASP.NET Core web application.

For this repo, we used `Directory.Build.Props` to provide the ability for each microservice to produce an OpenAPI 3.x Specification using NSwag. Each microservice project will generate the `specification.json` file at build time and store it in the `/wwwroot/api/v1/` folder.

A global `nswag.json` configuration file is used for producing the project specific OpenAPI spec documents. A set of variables specific to each project is passed to the `nswag` CLI to produce an appropriately described OpenAPI spec file.

To enable a project to automatically produce the OpenAPI spec file at build time, add the `ExportApiDocumentationOnBuild` property to a `PropertyGroup` in the `.csproj` file:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    ...
    <ExportApiDocumentationOnBuild>true</ExportApiDocumentationOnBuild>
    ...
  </PropertyGroup>
```

> Note: I chose NSwag + OpenApi3 instead of Swagger to ensure that the `x-enumNames` are used to describe `Enum` fields with string name equivalents. Having the string names for enum flags allows the CS/TS generated code to function exactly as expected.

- [ ] TODO: Generate C# / TypeScript client for API Gateway
- [ ] TODO: Publish Generated clients to NuGet / npmjs

## Configuring OpenAPI in ASP.NET Core WebAPI

In your `Program.cs`, add the following OpenAPI configuration:

```cs
// Enable Endpoint explorer to provide metadata for OpenAPI/Swagger document generation
builder.Services.AddEndpointsApiExplorer();

// Enable OpenAPI Document generation
builder.Services.AddOpenApiDocument(options =>
{
  options.DocumentName = "v1";
  options.Version = "v1";
  options.Title = "Users API";
  options.Description = "User Management Service";
});

var app = builder.Build();

// other middlewares...

// Serve static files - Since we generate the specification.json file at build time, we don't need to regenerate it at runtime. We simply serve the specification.json file as static content. 
// We also use the service specific route prefix so that requests to the spec would follow this pattern: https://<hostname>/<service-route-prefix>/api/v1/specification.json
//
// e.g. https://mydaprshop.com/users/api/v1/specification.json
//
app.UseStaticFiles(new StaticFileOptions()
{
  RequestPath = "/users"
});

app.UseSwaggerUi3(c =>
{
  c.Path = "/users/api";
  c.DocumentPath = "/users/api/v1/specification.json";
});
```

### Documenting your API

Describe your minimal endpoints or controller actions using the OpenAPI decorations.

For all available options, check out: [OpenAPI support in minimal API apps](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0)

For Minimal endpoints, describe a group of endpoints for a service by adding a tag with `WithTags()`. This will nicely group the operations together for each microservice. Useful for the CS/TS client generation and display in the Swagger UI.

```cs
// Tag a group of endpoints with a single tag using WithTags()
IEndpointRouteBuilder userRoutes = builder
  .MapGroup("users")
  .WithTags(new[] { "Users" });

// Add Operation Name/ID using WithName
userRoutes.MapGet("get", async (string username, [FromServices] UserService userService) =>
{
  User? user = await userService.GetUser(username);
  return user == null ? Results.NotFound() : Results.Ok(user);
})
  .WithName("GetUser");
```

## Merging APIs

Use the `merge-apis.ps1` script to produce a single OpenAPI Spec that includes operations from all the available microservices OpenAPI specs.

> **Warning**
> The `merge-apis.ps1` script is not very configurable right now as it does a straight forward deep merge of the OpenAPI json objects. This is particularly bad if two or more projects declare objects with the same name but different fields. For example, the `Order` class might be used described in multiple projects with different fields, but after the merge only one `Order` component will exist in the OpenAPI spec and it is not guaranteed to align with the service specific interface. Eventually, we want to end up with something like [API Matic - Merge Multiple API Definitions](https://docs.apimatic.io/manage-apis/api-merging/)

- [ ] TODO: Add property merge options (keep left, take right, rename-suffix)

## Useful Links

- [Building an Event-Driven .NET Core App with Dapr in .NET 7 Core](https://www.codemag.com/article/2303081)

- [Dapr .NET SDK Development with Docker-Compose](https://docs.dapr.io/developing-applications/sdks/dotnet/dotnet-development/dotnet-development-docker-compose/)

- [Microservices with Dapr mini-series (Pub/Sub + Tye)](https://swoopfunding.com/ca/swoop-engineering/microservices-with-dapr-mini-series-pub-sub-tye/)

- [Azure Container Apps - Workshop](https://azure.github.io/aca-dotnet-workshop/)

- [Dapr - As a microservice developer, finally focus on the application code again.](https://b-nova.com/en/home/content/how-microservice-developers-can-finally-concentrate-on-their-application-thanks-to-dapr)

- [Secure .NET microservices with Azure Container Apps and DAPR](https://medium.com/vx-company/secure-net-microservices-with-azure-container-apps-and-dapr-e122c6ea0aac)

- [ASP.NET Core Front-end + 2 Back-end APIs on Azure Container Apps](https://github.com/Azure-Samples/dotNET-FrontEnd-to-BackEnd-with-DAPR-on-Azure-Container-Apps/tree/main)
