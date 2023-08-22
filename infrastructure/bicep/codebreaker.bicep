targetScope = 'resourceGroup'

param defaultLocation string = 'West Europe'

resource resourceGroup 'Microsoft.Resources/resourceGroups@2022-09-01' existing = {
  name: 'codebreaker'
  scope: subscription()
}

module logAnalyticsWorkspace 'modules/log-analytics-workspace.bicep' = {
  name: 'log-analytics-workspace'
  scope: resourceGroup
  params: {
    name: 'codebreakerinsights'
    location: defaultLocation
  }
}

module containerRegistry 'modules/container-registry.bicep' = {
  name: 'container-registry'
  scope: resourceGroup
  params: {
    name: 'codebreaker'
    location: defaultLocation
  }
}

var cosmosDatabaseAccountName = 'codebreaker'
module cosmos 'modules/cosmos.bicep' = {
  name: 'cosmos'
  scope: resourceGroup
  params: {
    databaseAccountName: cosmosDatabaseAccountName
    location: defaultLocation
  }
}

module containerAppEnvironment 'modules/container-app-environment.bicep' = {
  name: 'container-app-environment'
  scope: resourceGroup
  params: {
    name: 'codebreakerenv'
    location: defaultLocation
    logAnalyticsWorkspaceName:logAnalyticsWorkspace.outputs.name
  }
}

module gameApiContainerApp 'modules/game-api-container-app.bicep' = {
  name: 'codebreaker-gamesapi-3'
  scope: resourceGroup
  params: {
    containerAppEnvironmentId: containerAppEnvironment.outputs.id
    name: 'codebreaker-gamesapi-3'
    location: defaultLocation
    databaseAccountName: cosmosDatabaseAccountName
    containerImage: 'codebreaker.azurecr.io/codebreaker-gamesapi:latest'
  }
}
