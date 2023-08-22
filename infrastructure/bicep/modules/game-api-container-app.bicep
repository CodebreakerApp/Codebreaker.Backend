// Parameters
@description('The id of the container app environment')
param containerAppEnvironmentId string

@description('The name for the container app')
param name string = 'game-api'

@description('Specifies the container image to deploy for the container app\nExample: \'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest\'')
param containerImage string

@description('Azure Cosmos DB account name')
@maxLength(44)
param databaseAccountName string

@description('The location for the container app')
param location string = resourceGroup().location

@description('The target port for the container app')
param port int = 443

@description('Number of CPU cores the container can use. Can be with a maximum of two decimals.')
param cpu string = '0.25'

@description('Amount of memory (in gibibytes, GiB) allocated to the container up to 4GiB. Can be with a maximum of two decimals. Ratio with CPU cores must be equal to 2.')
param memory string = '0.5'

@description('Minimum number of replicas the container app will be deployed')
@minValue(0)
@maxValue(300)
param minReplicas int = 0

@description('Maximum number of replicas the container app will be deployed')
@minValue(1)
@maxValue(300)
param maxReplicas int = 10

// Templates
resource gameApiContainerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: name
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    managedEnvironmentId: containerAppEnvironmentId
    configuration: {
      ingress: {
        external: true
        targetPort: port
        allowInsecure: false
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
      }
    }
    template: {
      containers: [
        {
          name: name
          image: containerImage
          resources: {
            cpu: json(cpu)
            memory: '${memory}Gi'
          }
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
      }
    }
  }
}



resource databaseAccount 'Microsoft.DocumentDB/databaseAccounts@2023-04-15' existing = {
  name: toLower(databaseAccountName)
}

resource cosmosDataContributorRole 'Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions@2023-04-15' existing = {
  parent: databaseAccount
  name: '00000000-0000-0000-0000-000000000002'  // Cosmos data contributor
}

resource gameApiContainerAppComsosDataContributorRoleAssignment 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2023-04-15' = {
  parent: databaseAccount
  name: guid(databaseAccount.id, name)
  properties: {
    roleDefinitionId: cosmosDataContributorRole.id
    principalId: gameApiContainerApp.identity.principalId
    scope: databaseAccount.id
  }
}
