param containerAppsEnvironmentName string

@secure()
param serviceBusConnectionString string

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2022-03-01' existing = {
  name: containerAppsEnvironmentName

  resource daprComponent 'daprComponents@2022-03-01' = {
    name: 'daprshop-pubsub'
    properties: {
      componentType: 'pubsub.azure.servicebus'
      version: 'v1'
      secrets: [
        {
          name: 'service-bus-connection-string'
          value: serviceBusConnectionString
        }
      ]
      metadata: [
        {
          name: 'connectionString'
          secretRef: 'service-bus-connection-string'
        }
      ]
      scopes: [
        'users-api'
        'cart-api'
        'products-api'
        'orders-api'
        'notifications-api'
      ]
    }
  }
}

output daprPubSubName string = containerAppsEnvironment::daprComponent.name
