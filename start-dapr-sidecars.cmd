:: dotnet build

:: merge all the spec files into a single spec for the gateway
::pwsh merge-apis.ps1
npx openapi-merge-cli

:: start all the api services
start "users-api"			dapr run --app-id users-api			--app-port 7108 --app-protocol https --resources-path ./components/ -- dotnet run --no-build -lp https --project ./DaprShop.UserManagement.API/DaprShop.UserManagement.API.csproj
start "cart-api"			dapr run --app-id cart-api			--app-port 7283 --app-protocol https --resources-path ./components/ -- dotnet run --no-build -lp https --project ./DaprShop.ShoppingCart.API/DaprShop.ShoppingCart.API.csproj
start "orders-api"			dapr run --app-id orders-api		--app-port 7233 --app-protocol https --resources-path ./components/ -- dotnet run --no-build -lp https --project ./DaprShop.Orders.API/DaprShop.Orders.API.csproj
start "products-api"		dapr run --app-id products-api		--app-port 7084 --app-protocol https --resources-path ./components/ -- dotnet run --no-build -lp https --project ./DaprShop.Products.API/DaprShop.Products.API.csproj
start "notifications-api"	dapr run --app-id notifications-api	--app-port 7196 --app-protocol https --resources-path ./components/ -- dotnet run --no-build -lp https --project ./DaprShop.Notifications.API/DaprShop.Notifications.API.csproj

:: start the gateway service (ensure dapr http port is set to 3500)
start "gateway-api"			dapr run --app-id gateway-api		--app-port 7192 --app-protocol https --resources-path ./components/ --dapr-http-port 3500 -- dotnet run --no-build -lp https --project ./DaprShop.Gateway.API/DaprShop.Gateway.API.csproj
