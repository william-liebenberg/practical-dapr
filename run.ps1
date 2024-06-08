dotnet build .\DaprShop.sln

#Start-Sleep -Seconds 2
# ./merge-apis.ps1
npx openapi-merge-cli

#Start-Sleep -Seconds 2

dapr run -f dapr.yaml