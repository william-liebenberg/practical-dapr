param location string
param containerAppsEnvironmentId string
param managedIdentityId string

param registry string
param registryUsername string
@secure()
param registryPassword string

@secure()
@description('The Application Insights Instrumentation Key.')
param appInsightsInstrumentationKey string

param cartApiFqdn string
param productsApiFqdn string
param ordersApiFqdn string
param usersApiFqdn string
param notificationsApiFqdn string

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
              name: 'ApplicationInsights__InstrumentationKey'
              value: appInsightsInstrumentationKey
            }
            {
              name: 'ApiRoutes__0__HostUrl'
              value: 'https://${cartApiFqdn}'
            }
            {
              name: 'ApiRoutes__1__HostUrl'
              value: 'https://${productsApiFqdn}'
            }
            {
              name: 'ApiRoutes__2__HostUrl'
              value: 'https://${ordersApiFqdn}'
            }
            {
              name: 'ApiRoutes__3__HostUrl'
              value: 'https://${usersApiFqdn}'
            }
            {
              name: 'ApiRoutes__4__HostUrl'
              value: 'https://${notificationsApiFqdn}'
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
