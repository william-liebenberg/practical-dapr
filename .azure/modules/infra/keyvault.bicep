param location string
param uniqueSeed string
param vaultName string = 'kv-${uniqueSeed}'
param managedIdentityObjectId string

// param catalogDbConnectionString string
// param orderingDbConnectionString string
// param identityDbConnectionString string

resource keyVault 'Microsoft.KeyVault/vaults@2023-02-01' = {
  name: vaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: managedIdentityObjectId
        permissions: {
          secrets: [
            'get'
            'list'
            'set'
            'delete'
          ]
        }
      }
    ]
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: false
    enableSoftDelete: false
  }
}

// resource catalogDbConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2022-11-01' = {
//   parent: keyVault
//   name: 'catalogDbConnectionString'
//   properties: {
//     value: catalogDbConnectionString
//   }
// }

// resource orderDbConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2022-11-01' = {
//   parent: keyVault
//   name: 'orderDbConnectionString'
//   properties: {
//     value: orderingDbConnectionString
//   }
// }

// resource identityDbConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2022-11-01' = {
//   parent: keyVault
//   name: 'identityDbConnectionString'
//   properties: {
//     value: identityDbConnectionString
//   }
// }

output vaultName string = keyVault.name

