$resourceGroupName = "aca-dapr-shop"

az group create --name $resourceGroupName --location australiaeast

az deployment group create --resource-group $resourceGroupName --template-file main.bicep