$resourceGroupName = "aca-dapr-shop"
$registryName="acadaprshop.azurecr.io"
$registryUsername="acadaprshop"
$registryPassword="LnQMuZkavfgFuF+aFwI//mUQDZ6RxT0S7IV6IfUofs+ACRCeJbH9"
$subscriptionId="5fb293a6-c2ac-4ee7-bad0-15d8f4376034"
$sendgridApiKey="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"

az group create --name $resourceGroupName --location australiasoutheast

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