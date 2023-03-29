param location string
param containerAppsEnvironmentId string
param managedIdentityId string

param registry string
param registryUsername string
@secure()
param registryPassword string

resource cartApiContainerApp 'Microsoft.App/containerApps@2022-10-01' = {
  name: 'cart-api'
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
          name: 'cart-api'
          //image: 'acadaprshop.azurecr.io/daprshop-shoppingcart-api'
          image: 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
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
        appId: 'cart-api'
        appPort: 80
      }
      ingress: {
        external: true
        targetPort: 80
        allowInsecure: true
        corsPolicy: {
          allowedOrigins: [
            '*'
          ]
          allowedHeaders: [
            '*'
          ]
          allowedMethods: [
            '*'
          ]
        }
      }
    }
  }
}
