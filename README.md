# Codebreaker

[![API](https://github.com/CNILearn/codebreaker/actions/workflows/codebreakerapi-AutoDeployTrigger-ee54dca3-868c-4c78-9b6c-72e2c6719e10.yml/badge.svg)](https://github.com/CNILearn/codebreaker/actions/workflows/codebreakerapi-AutoDeployTrigger-ee54dca3-868c-4c78-9b6c-72e2c6719e10.yml)

[![Bot](https://github.com/CNILearn/codebreaker/actions/workflows/codebreaker-bot.yml/badge.svg)](https://github.com/CNILearn/codebreaker/actions/workflows/codebreaker-bot.yml)

[![Live](https://github.com/CNILearn/codebreaker/actions/workflows/codebreaker-live.yml/badge.svg)](https://github.com/CNILearn/codebreaker/actions/workflows/codebreaker-live.yml)

[![Blazor App](https://github.com/CNILearn/codebreaker/actions/workflows/azure-static-web-apps-ambitious-smoke-0612ff603.yml/badge.svg)](https://github.com/CNILearn/codebreaker/actions/workflows/azure-static-web-apps-ambitious-smoke-0612ff603.yml)

[![Android](https://github.com/CNILearn/codebreaker/actions/workflows/codebreaker-maui-android.yml/badge.svg)](https://github.com/CNILearn/codebreaker/actions/workflows/codebreaker-maui-android.yml)

[![Data NuGet](https://github.com/CNinnovation/codebreaker/actions/workflows/codebreaker-lib-data.yml/badge.svg)](https://github.com/CNinnovation/codebreaker/actions/workflows/codebreaker-lib-data.yml)

[![Services Client NuGet](https://github.com/CNinnovation/codebreaker/actions/workflows/codebreaker-services.yml/badge.svg)](https://github.com/CNinnovation/codebreaker/actions/workflows/codebreaker-services.yml)

[![Shared NuGet](https://github.com/CNinnovation/codebreaker/actions/workflows/codebreaker-shared.yml/badge.svg)](https://github.com/CNinnovation/codebreaker/actions/workflows/codebreaker-shared.yml)

[![MVVM NuGet](https://github.com/CNinnovation/codebreaker/actions/workflows/codebreaker-lib-viewmodels.yml/badge.svg)](https://github.com/CNinnovation/codebreaker/actions/workflows/codebreaker-lib-viewmodels.yml)

[Guidelines](guidelines.md)

[Codebreaker Packages Feed](https://pkgs.dev.azure.com/cnilearn/codebreakerpackages/_packaging/codebreaker/nuget/v3/index.json)

## Codebreaker Services

* REST API to play games, writes information to Cosmos
* Bot who plays games calling the API. The bot can be invoked calling commands from a REST API
* REST API for reporting

## Codebreaker Client apps

* Blazor app to show who played (currently, in the future games can be played using Blazor)
* WinUI app to play games calling the API
* WPF app to play games calling the API

## Azure Services in use

* Azure Container Apps
* Azure Cosmos DB

## To be defined and developed

* Authentication
* Database cleanup-service - running with a timer to cleanup the database
* SignalR service showing live information about running games (Notification about game moves and results via Azure Event Grid)
* .NET MAUI client app

## Azure Services that need to be used

* Azure Message Queue (an alternative trigger for the Bot)
* Azure App Configuration
* Azure Key Vault
* Azure Active Directory B2C
* Azure Event Grid

## Alternative Implementations

* Platform Uno app
* Avalonia app
* Services using Dapr
