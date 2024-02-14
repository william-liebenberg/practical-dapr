$resourceGroupName = "aca-dapr-shop-ndc"
$registryName="acadaprshop.azurecr.io"
$registryUsername="acadaprshop"
$registryPassword="F5pzejsBAa/tbVM4tuPxunrUvxmypV+kyI+sFVJV1h+ACRDyLavJ"
$subscriptionId="5fb293a6-c2ac-4ee7-bad0-15d8f4376034"
$sendgridApiKey="SG.xxx"

# az group create --name $resourceGroupName --location australiaeast

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