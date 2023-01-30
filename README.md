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

## Guidelines

[Guidelines](guidelines.md)

## Codebreaker Package Feed

[Codebreaker Packages Feed](https://pkgs.dev.azure.com/cnilearn/codebreakerpackages/_packaging/codebreaker/nuget/v3/index.json)

## Codebreaker Services

* REST API to play games, writes information to Cosmos
* Bot who plays games calling the API. The bot can be invoked calling commands from a REST API
* REST API for reporting
* SignalR Services to show live games

## Codebreaker Client apps

* Blazor app to play games and show game results using [MudBlazor](https://www.mudblazor.com/), [FastBlazor](https://github.com/microsoft/fast-blazor), and native, pure Blazor with only CSS
* WinUI app to play games calling the API, and show live services
* WPF app to play games calling the API
* .NET MAUI App to play games calling the API (Android, iOS, Windows)

## Azure Services in use

* Azure Container Apps
* Azure Cosmos DB
* Azure Active Directory B2C
* Azure SignalR Services
* Azure App Configuration
* Azure Event Hub
* Azure App Services

## To be defined and developed

* Authentication with Microsoft, Facebook, Google accounts
* Database cleanup-service - running with a timer to cleanup the database
* Platform Uno client
* Services using Dapr
* Grpc alternative for Game API

## More Azure Services that will be used

* Azure Message Queue (an alternative trigger for the Bot)
* Azure Key Vault
* Azure Event Grid
