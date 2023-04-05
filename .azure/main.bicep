param location string = resourceGroup().location
param envName string = 'test'

param registry string
param registryUsername string
@secure()
param registryPassword string

param uniqueSeed string = '${take(uniqueString(resourceGroup().id, deployment().name), 6)}-${envName}'

// --------------------------------------------------------------------------
// Core Infrastructure
// --------------------------------------------------------------------------

module managedIdentity 'modules/infra/managed-identity.bicep' = {
  name: '${deployment().name}-infra-managed-identity'
  params: {
    location: location
    uniqueSeed: uniqueSeed
  }
}

module containerAppsEnvironment 'modules/infra/container-app-environment.bicep' = {
  name: '${deployment().name}-infra-container-app-env'
  params: {
    location: location
    uniqueSeed: uniqueSeed
  }
}

module cosmos 'modules/infra/cosmos.bicep' = {
  name: '${deployment().name}-infra-cosmos-db'
  params: {
    location: location
    uniqueSeed: uniqueSeed
  }
}

module serviceBus 'modules/infra/service-bus.bicep' = {
  name: '${deployment().name}-infra-service-bus'
  params: {
    location: location
    uniqueSeed: uniqueSeed
  }
}

module keyVault 'modules/infra/keyvault.bicep' = {
  name: '${deployment().name}-infra-keyvault'
  params: {
    location: location
    uniqueSeed: uniqueSeed
    managedIdentityObjectId: managedIdentity.outputs.identityObjectId
    // catalogDbConnectionString: sqlServer.outputs.catalogDbConnectionString
  }
}

// --------------------------------------------------------------------------
// Dapr components
// --------------------------------------------------------------------------

module daprPubSub 'modules/dapr/pubsub.bicep' = {
  name: '${deployment().name}-dapr-pubsub'
  params: {
    containerAppsEnvironmentName: containerAppsEnvironment.outputs.name
    serviceBusConnectionString: serviceBus.outputs.connectionString
  }
}

module daprStateStore 'modules/dapr/state-store.bicep' = {
  name: '${deployment().name}-dapr-statestore'
  params: {
    containerAppsEnvironmentName: containerAppsEnvironment.outputs.name
    cosmosDbName: cosmos.outputs.cosmosDbName
    cosmosCollectionName: cosmos.outputs.cosmosCollectionName
    cosmosUrl: cosmos.outputs.cosmosUrl
    cosmosAccountName: cosmos.outputs.cosmosAccountName
  }
}

module daprSecretStore 'modules/dapr/secret-store.bicep' = {
  name: '${deployment().name}-dapr-secretstore'
  params: {
    containerAppsEnvironmentName: containerAppsEnvironment.outputs.name
    vaultName: keyVault.outputs.vaultName
    managedIdentityClientId: managedIdentity.outputs.identityClientId
  }
}

// --------------------------------------------------------------------------
// Container apps
// --------------------------------------------------------------------------

module usersApi 'modules/apps/users-api.bicep' = {
  name: '${deployment().name}-app-users-api'
  dependsOn: [
    daprPubSub
    daprStateStore
  ]
  params: {
    location: location
    containerAppsEnvironmentId: containerAppsEnvironment.outputs.id
    managedIdentityId: managedIdentity.outputs.identityId
    registry: registry
    registryUsername: registryUsername
    registryPassword: registryPassword
  }
}


module cartApi 'modules/apps/cart-api.bicep' = {
  name: '${deployment().name}-app-cart-api'
  dependsOn: [
    daprPubSub
    daprStateStore
  ]
  params: {
    location: location
    containerAppsEnvironmentId: containerAppsEnvironment.outputs.id
    managedIdentityId: managedIdentity.outputs.identityId
    registry: registry
    registryUsername: registryUsername
    registryPassword: registryPassword
  }
}


module productsApi 'modules/apps/products-api.bicep' = {
  name: '${deployment().name}-app-products-api'
  dependsOn: [
    daprPubSub
    daprStateStore
  ]
  params: {
    location: location
    containerAppsEnvironmentId: containerAppsEnvironment.outputs.id
    managedIdentityId: managedIdentity.outputs.identityId
    registry: registry
    registryUsername: registryUsername
    registryPassword: registryPassword
  }
}

module ordersApi 'modules/apps/orders-api.bicep' = {
  name: '${deployment().name}-app-orders-api'
  dependsOn: [
    daprPubSub
    daprStateStore
  ]
  params: {
    location: location
    containerAppsEnvironmentId: containerAppsEnvironment.outputs.id
    managedIdentityId: managedIdentity.outputs.identityId
    registry: registry
    registryUsername: registryUsername
    registryPassword: registryPassword
  }
}


module gatewayApi 'modules/apps/gateway-api.bicep' = {
  name: '${deployment().name}-app-gateway-api'
  dependsOn: [
    daprPubSub
    daprStateStore
  ]
  params: {
    location: location
    containerAppsEnvironmentId: containerAppsEnvironment.outputs.id
    managedIdentityId: managedIdentity.outputs.identityId

    registry: registry
    registryUsername: registryUsername
    registryPassword: registryPassword

    cartApiFqdn: cartApi.outputs.fqdn
    productsApiFqdn: productsApi.outputs.fqdn
    ordersApiFqdn: ordersApi.outputs.fqdn
    usersApiFqdn: usersApi.outputs.fqdn
  }
}
