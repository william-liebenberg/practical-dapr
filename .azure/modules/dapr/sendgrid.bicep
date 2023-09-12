param containerAppsEnvironmentName string

@secure()
param sendgridApiKey string

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2023-05-02-preview' existing = {
  name: containerAppsEnvironmentName

  resource daprComponent 'daprComponents@2023-05-02-preview' = {
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
