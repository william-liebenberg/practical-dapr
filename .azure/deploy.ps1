$resourceGroupName = "aca-dapr-shop"
$registryName="acadaprshop.azurecr.io"
$registryUsername="acadaprshop"
$registryPassword="tu3ihs+ssR5iu22voqRmmYOqkWWvjxk2hPuQhtfR6r+ACRCqMzGh"

az group create --name $resourceGroupName --location australiaeast

az deployment group create --resource-group $resourceGroupName --template-file main.bicep --parameters registry=$registryName registryUsername=$registryUsername registryPassword=$registryPassword

# az containerapp create `
#     --name gateway-api `
#     --resource-group $resourceGroupName `
#     --environment $resourceGroupName `
#     --image $registryName/sampleapp:'$(Build.BuildId)' `
#     --min-replicas 1 `
#     --max-replicas 1 `
#     --registry-server $registryName `
#     --registry-username $registryUsername `
#     --registry-password $registryUsername `
#     --target-port 80 `
#     --ingress 'external'