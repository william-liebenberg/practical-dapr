dotnet build .\DaprShop.sln

# ./merge-apis.ps1
npx openapi-merge-cli

dapr run -f dapr.yaml