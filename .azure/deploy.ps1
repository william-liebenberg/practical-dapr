$resourceGroupName = "aca-dapr-shop"
$registryName="acadaprshop.azurecr.io"
$registryUsername="acadaprshop"
$registryPassword=""
$sendgridApiKey=""

az group create `
    --name $resourceGroupName `
    --location australiaeast

az deployment group create `
    --resource-group $resourceGroupName `
    --template-file main.bicep `
    --parameters registry=$registryName registryUsername=$registryUsername registryPassword=$registryPassword sendgridApiKey=$sendgridApiKey
