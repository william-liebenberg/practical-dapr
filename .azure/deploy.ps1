$resourceGroupName = "aca-dapr-shop"
$registryName="acadaprshop.azurecr.io"
$registryUsername="acadaprshop"
$registryPassword="xxx"
$subscriptionId="xxx"
$sendgridApiKey="SG.xxx"

az group create --name $resourceGroupName --location australiaeast

az deployment group create `
    --resource-group $resourceGroupName `
    --template-file main.bicep `
    --parameters location=australiaeast `
        registry=$registryName `
        registryUsername=$registryUsername `
        registryPassword=$registryPassword `
        sendgridApiKey=$sendgridApiKey

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