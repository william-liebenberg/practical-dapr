# Practical Microservice Development with Dapr

## Install Dapr CLI

```ps1

winget install Dapr.CLI.Preview

dapr init --runtime-version 1.9.5


```

## Install Dapr VSCode Extension

- check out the dashboard - http://localhost:8000/overview

## Do some local development

```sh
dapr run --app-id myapp --dapr-http-port 3500 --components-path ./components

Invoke-RestMethod -Uri 'http://localhost:3500/v1.0/secrets/my-secret-store/my-secret'

Invoke-RestMethod -Uri 'http://localhost:3500/v1.0/secrets/my-secret-store/nested-secret:field1'

```

## Sample Application

```sh
# Create a new WebAPI
dotnet new webapi -n DaprShop.ShoppingCart.API
cd DaprShop.ShoppingCart.API

# Add Dapr packages for ASP.NET Core
dotnet add DaprShop.ShoppingCart.API.csproj package Dapr.Client

dotnet add DaprShop.ShoppingCart.API.csproj package Dapr.AspNetCore

# clean up the original sample code
cd Controllers
remove-item WeatherForecastController.cs

# Add some domain entities
cd..
mkdir Domain
cd Domain

new-item ShoppingCartItem.cs
new-item ShoppingCart.cs

# Add some services
cd ..
mkdir Services
cd Services
new-item ShoppingCartService.cs

# Add some controllers
cd ..
cd Controllers
new-item ShoppingCartController.cs


```


## Push to GitHub

## Automate the builds

Build app using Docker.

Push to ACR.

## Deploy to Azure




## Running a dapr app

https://www.codemag.com/article/2303081

```ps1
dapr run --app-id dapr-service --resources-path ../local-components/ --app-port 5190 -- dotnet run --project .

dapr run --app-id shopping-cart-api --resources-path ../local-components/ --app-port 6000 -- dotnet run

# run dapr daemon directly, then F5 to debug the app
daprd --app-id shopping-cart-api --resources-path ../local-components/ --app-port 7000

```

https://docs.dapr.io/developing-applications/sdks/dotnet/dotnet-development/dotnet-development-tye/

https://swoopfunding.com/ca/swoop-engineering/microservices-with-dapr-mini-series-pub-sub-tye/

https://markheath.net/post/running-locally-with-dapr-options

https://code.benco.io/dapr-store/


## Services

### Cart Service

- `POST /cart/add/{userId}/{productid}/{quantity}`
  - add a number of products in the cart of a given user
- `GET /cart/{userid}`
  - cart for user
- `POST /cart/clear/{userid}`
  - clear a users cart
- `POST /cart/submit/{userid}`
  - submit a cart and turn it into an Order

#### Dapr Interaction

- Pub/Sub
  - The cart pushes Order entities to the orders-queue topic to be collected by the orders service
- State
  - Stores and retrieves Cart entities from the state service, keyed on username.
- Service Invocation
  - Cross service call to products API to lookup and check products in the cart

### Orders Service

Aka Kitchen Service / Manufacturing Service.

The service provides some fake order processing activity so that orders are moved through a number of statuses, simulating some back-office systems or inventory management.

Orders are initially set to `OrderReceived` status, then after 30 seconds moved to `OrderProcessing`, then after 2 minutes moved to `OrderComplete`.


/get/{id}                GET a single order by orderID
/getForUser/{userId}   GET all orders for a given user


#### Dapr Interaction

- Pub/Sub
  - Subscribes to the `orders-queue` topic to receive new orders from the cart service
- State
  - Stores and retrieves `Order` entities from the state service, keyed on `OrderID`. Also lists of orders per user, held as an array of OrderIDs and keyed on username
- Bindings
  - All output bindings are optional, the service operates without these present
- Azure Blob
  - For saving “order reports” as text files into Azure Blob storage
- SendGrid
  - For sending emails to users via SendGrid


### Users service

Simple user profile service. Only registered users can use the store to place orders etc.

/register               POST a new user to register them
/get/{userId}           GET the user profile for given user
/private/get/{userId}   GET the user profile for given user. Private endpoints are NOT exposed through the gateway
/isregistered/{userId}  GET the registration status for a given user

The service is notable as it consists of a mix of both secured API routes, and two that are anonymous/open /isregistered and /private/get

#### Dapr Integration

- State
  - Stores and retrieves User entities from the state service, keyed on username.