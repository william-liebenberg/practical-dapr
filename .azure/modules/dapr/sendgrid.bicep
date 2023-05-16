param containerAppsEnvironmentName string

@secure()
param sendgridApiKey string

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2022-03-01' existing = {
  name: containerAppsEnvironmentName

  resource daprComponent 'daprComponents@2022-03-01' = {
    name: 'sendgrid'
    properties: {
      componentType: 'bindings.twilio.sendgrid'
      version: 'v1'
      secrets: [
        {
          name: 'sendgridapikey'
          value: sendgridApiKey
        }
      ]
      metadata: [
        {
          name: 'apiKey'
          secretRef: 'sendgridapikey'
        }
      ]
      scopes: [
        'notifications-api'
      ]
    }
  }
}

output sendgridName string = containerAppsEnvironment::daprComponent.name
