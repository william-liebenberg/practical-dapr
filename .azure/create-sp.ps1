$subscriptionId="xxx"
$resourceGroupName = "aca-dapr-shop"

az ad sp create-for-rbac --name "acap-dapr-shop-gh" --role contributor `
    --scopes /subscriptions/$subscriptionId/resourceGroups/$resourceGroupName `
    --json-auth