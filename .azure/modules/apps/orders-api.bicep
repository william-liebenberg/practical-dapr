param location string
param containerAppsEnvironmentId string
param managedIdentityId string

param registry string
param registryUsername string
@secure()
param registryPassword string

resource ordersApiContainerApp 'Microsoft.App/containerApps@2022-10-01' = {
  name: 'orders-api'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
    }
  }

  properties: {
    managedEnvironmentId: containerAppsEnvironmentId
    template: {
      containers: [
        {
          name: 'orders-api'
          image: 'acadaprshop.azurecr.io/daprshop-orders-api'
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://0.0.0.0:80'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
    configuration: {
      secrets: [
        {
          name: 'containerregistrypasswordref'
          value: registryPassword
        }
      ]
      registries: [
        {
          // server is in the format of myregistry.azurecr.io
          server: registry
          username: registryUsername
          passwordSecretRef: 'containerregistrypasswordref'
        }
      ]
      activeRevisionsMode: 'single'
      dapr: {
        enabled: true
        appId: 'orders-api'
        appPort: 80
      }
      ingress: {
        external: true
        targetPort: 80
        allowInsecure: true
      }
    }
  }
}
