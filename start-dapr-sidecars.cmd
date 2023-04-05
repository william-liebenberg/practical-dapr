start "gateway-api" dapr run --dapr-http-port 3500 --app-ssl --components-path ./components/ --app-id gateway-api -- dotnet run -lp https --project ./DaprShop.Gateway.API/DaprShop.Gateway.API.csproj
start "users-api" dapr run --app-port 7108 --app-ssl --components-path ./components/ --app-id users-api -- dotnet run -lp https --project ./DaprShop.UserManagement.API/DaprShop.UserManagement.API.csproj
start "cart-api" dapr run --app-port 7283 --app-ssl --components-path ./components/ --app-id cart-api -- dotnet run -lp https --project ./DaprShop.ShoppingCart.API/DaprShop.ShoppingCart.API.csproj
start "orders-api" dapr run --app-port 7233 --app-ssl --components-path ./components/ --app-id orders-api -- dotnet run -lp https --project ./DaprShop.Orders.API/DaprShop.Orders.API.csproj
start "products-api" dapr run --app-port 7084 --app-ssl --components-path ./components/ --app-id products-api -- dotnet run -lp https --project ./DaprShop.Products.API/DaprShop.Products.API.csproj
