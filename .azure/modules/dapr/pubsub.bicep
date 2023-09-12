param containerAppsEnvironmentName string

//@secure()
//param serviceBusConnectionString string

param serviceBusNamespaceName string

resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' existing = {
  name: serviceBusNamespaceName
}

var serviceBusConnectionString = 'Endpoint=sb://${serviceBus.name}.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=${listKeys('${serviceBus.id}/AuthorizationRules/RootManageSharedAccessKey', serviceBus.apiVersion).primaryKey}'

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2023-05-02-preview' existing = {
  name: containerAppsEnvironmentName

  resource daprComponent 'daprComponents@2023-05-02-preview' = {
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
