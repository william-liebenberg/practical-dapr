@rem start "user-management-api" daprd -app-port 7001 -app-protocol HTTP -metrics-port 54298 -dapr-internal-grpc-port 54297 -dapr-grpc-port 54297 -http-port 54296 -resources-path ./DaprShop.UserManagement.API/.dapr/components/ -app-id users-api
@rem start "shopping-cart-api"   daprd -app-port 7000 -app-protocol HTTP -metrics-port 9091 -dapr-internal-grpc-port 51301 -dapr-grpc-port 64421 -dapr-http-port 3501 -resources-path ./DaprShop.ShoppingCart.API/.dapr/components/ -app-id shopping-cart-api

@rem start "cart-api"   dapr run --app-port 18000 --components-path ./DaprShop.ShoppingCart.API/.dapr/components/ --app-id cart-api
@rem start "users-api"   dapr run --app-port 17000 --components-path ./DaprShop.UserManagement.API/.dapr/components/ --app-id users-api

@rem start "cart-api"   daprd  --app-port 18000 -metrics-port 30000 -dapr-grpc-port 50001 -dapr-http-port 33000 --components-path ./DaprShop.ShoppingCart.API/.dapr/components/ --app-id cart-api
start "users-api"   daprd  --app-port 17000 -metrics-port 30100 -dapr-grpc-port 50100 -dapr-http-port 33100 --components-path ./DaprShop.UserManagement.API/.dapr/components/ --app-id users-api

@rem start "user-management-api" dapr run --app-port 7001 --app-protocol HTTP --components-path ./DaprShop.UserManagement.API/.dapr/components/ --app-id users-api
@rem start "shopping-cart-api"   dapr run --app-port 7000 --app-protocol HTTP --components-path ./DaprShop.ShoppingCart.API/.dapr/components/ --app-id shopping-cart-api