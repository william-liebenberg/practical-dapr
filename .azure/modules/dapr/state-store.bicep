param containerAppsEnvironmentName string

param cosmosAccountName string
param cosmosDbName string
param cosmosCollectionName string
param cosmosUrl string

// @secure()
// param cosmosKey string

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2022-11-15' existing = {
  name: cosmosAccountName
}

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2022-10-01' existing = {
  name: containerAppsEnvironmentName

  resource daprComponent 'daprComponents@2022-10-01' = {
    name: 'daprshop-statestore'
    properties: {
      componentType: 'state.azure.cosmosdb'
      version: 'v1'
      secrets: [
        {
          name: 'cosmos-key'
          value: cosmosAccount.listKeys().primaryMasterKey
        }
      ]
      metadata: [
        {
          name: 'url'
          value: cosmosUrl
        }
        {
          name: 'masterKey'
          secretRef: 'cosmos-key'
        }
        {
          name: 'database'
          value: cosmosDbName
        }
        {
          name: 'collection'
          value: cosmosCollectionName
        }
        {
          name: 'actorStateStore'
          value: 'true'
        }
      ]
      scopes: [
        'cart-api'
        'users-api'
        'products-api'
        'orders-api'
      ]
    }
  }
}

output daprStateStoreName string = containerAppsEnvironment::daprComponent.name
