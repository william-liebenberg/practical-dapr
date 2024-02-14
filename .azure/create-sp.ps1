$subscriptionId="5fb293a6-c2ac-4ee7-bad0-15d8f4376034"
$resourceGroupName = "aca-dapr-shop"

az ad sp create-for-rbac --name "acap-dapr-shop-gh" --role contributor `
    --scopes /subscriptions/$subscriptionId/resourceGroups/$resourceGroupName `
    --json-auth