# Codebreaker Backends

This is the GitHub repo for the backend services of the **Codebreaker** solution.

See these repositories for clients:

* [WinUI, .NET MAUI, WPF](https://github.com/codebreakerapp/Codebreaker.Xaml)
* [Blazor](https://github.com/codebreakerapp/Codebreaker.Blazor)

See this repository for the book about the backend:

[Pragmatic Microservices with C# and Azure](https://github.com/CodebreakerApp/Pragmatic-Microservices-With-C-Sharp-and-Azure)

## Version 3

This repository is just in progress to update all the backend services to version 3 (along with the book repo). What can be used yet?

### Games API

The new Games API service with access to SQL Server and Azure Cosmos DB. Currently, you need to run this locally. A hosted version will be available with a later itation (see the progress in the book repo).

### Analyzers Library

A NuGet package for the library **CNinnovation.Codebreaker.Analyzers** (preview version) is published to the NuGet server. This library is used by the games API, and you need it creating your own custom games. The source code is available here.

### Client Library

A NugGet package for the library **CNinnovation.Codbreaker.Client** (preview version) is published to the NuGet server. This library is used by clients to call the games service.

## Builds

### Libraries

|Branch|Analyzers|Client Library|
|:--:|:--:|:--:|
**main**|[![Analyzers](https://github.com/CodebreakerApp/Codebreaker.Backend/actions/workflows/codebreaker-lib-analyzers.yml/badge.svg)](https://github.com/CodebreakerApp/Codebreaker.Backend/actions/workflows/codebreaker-lib-analyzers.yml)|[![Client Library](https://github.com/CodebreakerApp/Codebreaker.Backend/actions/workflows/codebreaker-lib-client.yml/badge.svg)](https://github.com/CodebreakerApp/Codebreaker.Backend/actions/workflows/codebreaker-lib-client.yml)

### APIs

|Banch|Games API|Bot|Live|User|
|:--:|:--:|:--:|:--:|:--:|
**main**|[![API](https://github.com/CNILearn/codebreaker/actions/workflows/codebreakerapi-AutoDeployTrigger-ee54dca3-868c-4c78-9b6c-72e2c6719e10.yml/badge.svg)](https://github.com/CNILearn/codebreaker/actions/workflows/codebreakerapi-AutoDeployTrigger-ee54dca3-868c-4c78-9b6c-72e2c6719e10.yml)|[![Bot](https://github.com/CNILearn/codebreaker/actions/workflows/codebreaker-bot.yml/badge.svg)](https://github.com/CNILearn/codebreaker/actions/workflows/codebreaker-bot.yml)|[![Live](https://github.com/CNILearn/codebreaker/actions/workflows/codebreaker-live.yml/badge.svg)](https://github.com/CNILearn/codebreaker/actions/workflows/codebreaker-live.yml)|[![User](https://github.com/CNILearn/codebreaker/actions/workflows/codebreaker-user.yml/badge.svg)](https://github.com/CNILearn/codebreaker/actions/workflows/codebreaker-user.yml)


### Integration Tests

|Branch|Game API|
|:--:|:--:|
**main**|[![Test Game API Integration](https://github.com/CNinnovation/codebreaker/actions/workflows/codebreakerapi-integrationtests.yml/badge.svg)](https://github.com/CNinnovation/codebreaker/actions/workflows/codebreakerapi-integrationtests.yml)

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

* [Blazor Pure CSS](https://codebreaker-pure.azurewebsites.net/)
* [Blazor MudBlazor](https://codebreaker-mud.azurewebsites.net/)
* [Blazor Microsoft.Fast](https://codebreaker-fast.azurewebsites.net/)

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
