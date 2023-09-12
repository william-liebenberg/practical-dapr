param uniqueSeed string
param location string
param identityName string = 'mid-${uniqueSeed}'

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: identityName
  location: location
}

output identityId string = managedIdentity.id
output identityClientId string = managedIdentity.properties.clientId
output identityObjectId string = managedIdentity.properties.principalId

