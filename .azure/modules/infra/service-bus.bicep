param location string
param uniqueSeed string

resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: 'sb-${uniqueSeed}'
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
}

//output connectionString string = 'Endpoint=sb://${serviceBus.name}.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=${listKeys('${serviceBus.id}/AuthorizationRules/RootManageSharedAccessKey', serviceBus.apiVersion).primaryKey}'
output serviceBusNamespaceName string = serviceBus.name
