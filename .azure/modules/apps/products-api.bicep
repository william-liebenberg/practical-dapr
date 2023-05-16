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

resource containerApp 'Microsoft.App/containerApps@2022-10-01' = {
  name: 'products-api'
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
          name: 'products-api'
          //image: 'acadaprshop.azurecr.io/daprshop-products-api'
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
        appId: 'products-api'
        appPort: 80
      }
      ingress: {
        external: false
        targetPort: 80
        allowInsecure: false
      }
    }
  }
}

output fqdn string = containerApp.properties.configuration.ingress.fqdn
