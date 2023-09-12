param location string
param uniqueSeed string
param cosmosAccountName string = 'cosmos-${uniqueSeed}'
param cosmosDbName string = 'daprShop'

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2023-04-15' = {
  name: cosmosAccountName
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: [
      {
        locationName: location
      }
    ]
    capabilities: [
      {
        name: 'EnableServerless'
      }
    ]
  }
}

resource cosmosDb 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-04-15' = {
  parent: cosmosAccount
  name: cosmosDbName
  properties: {
    resource: {
      id: cosmosDbName
    }
  }
}

resource cosmosCollection 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  parent: cosmosDb
  name: 'state'
  properties: {
    resource: {
      id: 'state'
      partitionKey: {
        paths: [
          '/partitionKey'
        ]
        kind: 'Hash'
      }
    }
  }
}

output cosmosAccountName string = cosmosAccount.name
output cosmosDbName string = cosmosDbName
output cosmosUrl string = cosmosAccount.properties.documentEndpoint
// output cosmosKey string = cosmosAccount.listKeys().primaryMasterKey
output cosmosCollectionName string = cosmosCollection.name
