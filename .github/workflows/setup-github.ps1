# variables
$subscriptionId=$(az account show --query id -o tsv)
$appName="aca-dapr-shop-gh"
$role="Contributor"

# Create AAD App and Service Principal and assign RBAC Role
(az ad sp create-for-rbac `
    --name $appName `
    --role $role `
    --scopes /subscriptions/$subscriptionId `
    --json-auth) | Out-File github.secrets.json

$json = Get-Content -Path github.secrets.json

#Write-Host $json

# Set the secret to github
$json | gh secret set AZURE_CREDENTIALS