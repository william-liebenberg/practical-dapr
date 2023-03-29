start "gateway-api" dapr run --dapr-http-port 3500 --components-path ./components/ --app-id gateway-api -- dotnet run --project ./DaprShop.Gateway.API/DaprShop.Gateway.API.csproj
start "users-api" dapr run --app-port 17000 --components-path ./components/ --app-id users-api -- dotnet run --project ./DaprShop.UserManagement.API/DaprShop.UserManagement.API.csproj
start "cart-api" dapr run --app-port 18000 --components-path ./components/ --app-id cart-api -- dotnet run --project ./DaprShop.ShoppingCart.API/DaprShop.ShoppingCart.API.csproj
start "orders-api" dapr run --app-port 19000 --components-path ./components/ --app-id orders-api -- dotnet run --project ./DaprShop.Orders.API/DaprShop.Orders.API.csproj
start "products-api" dapr run --app-port 20000 --components-path ./components/ --app-id products-api -- dotnet run --project ./DaprShop.Products.API/DaprShop.Products.API.csproj
