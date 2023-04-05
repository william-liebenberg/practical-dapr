param location string
param containerAppsEnvironmentId string
param managedIdentityId string

param registry string
param registryUsername string
@secure()
param registryPassword string

param cartApiFqdn string
param productsApiFqdn string
param ordersApiFqdn string
param usersApiFqdn string

resource containerApp 'Microsoft.App/containerApps@2022-10-01' = {
  name: 'gateway-api'
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
          name: 'gateway-api'
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
            {
              name: 'ApiRoutes__0__HostUrl'
              value: cartApiFqdn
            }
            {
              name: 'ApiRoutes__1__HostUrl'
              value: productsApiFqdn
            }
            {
              name: 'ApiRoutes__2__HostUrl'
              value: ordersApiFqdn
            }
            {
              name: 'ApiRoutes__3__HostUrl'
              value: usersApiFqdn
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
        appId: 'gateway-api'
        appPort: 80
      }
      ingress: {
        external: true
        targetPort: 80
        allowInsecure: false
      }
    }
  }
}

output fqdn string = containerApp.properties.configuration.ingress.fqdn
