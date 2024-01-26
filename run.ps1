dotnet build .\DaprShop.sln

./merge-apis.ps1

dapr run -f dapr.yaml