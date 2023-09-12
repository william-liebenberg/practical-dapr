param containerAppsEnvironmentName string
param vaultName string
param managedIdentityClientId string

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2023-05-02-preview' existing = {
  name: containerAppsEnvironmentName

  resource daprComponent 'daprComponents@2023-05-02-preview' = {
    name: 'daprshop-secretstore'
    properties: {
      componentType: 'secretstores.azure.keyvault'
      version: 'v1'
      secrets: [
        {
          name: 'azure-client-id'
          value: managedIdentityClientId
        }
      ]
      metadata: [
        {
          name: 'vaultName'
          value: vaultName
        }
        {
          name: 'azureEnvironment'
          value: 'AZUREPUBLICCLOUD'
        }
        {
          name: 'azureClientId'
          secretRef: 'azure-client-id'
        }
      ]
      scopes: [
        'users-api'
        'cart-api'
        'products-api'
        'orders-api'
      ]
    }
  }
}

output daprSecretStoreName string = containerAppsEnvironment::daprComponent.name
