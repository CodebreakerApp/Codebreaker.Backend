# Codebreaker Backends

This is the GitHub repo for the backend services of the **Codebreaker** solution.

See these repositories for clients:

* [WinUI, .NET MAUI, WPF](https://github.com/codebreakerapp/Codebreaker.Xaml)
* [Blazor](https://github.com/codebreakerapp/Codebreaker.Blazor)

See this repository for the book about the backend:

[Pragmatic Microservices with C# and Azure](https://github.com/PacktPublishing/Pragmatic-Microservices-with-CSharp-and-Azure/)

## Version 3

This repository has been updated with the source code from the book, but is updated and enhanced continuously.

### Games API

The new Games API service with access to SQL Server and Azure Cosmos DB. Currently, you need to run this locally. A hosted version will be available at a later date.

### Analyzers Library

The analyzers library **CNinnovation.Codebreaker.Analyzers** is used in the backend to return results based on a game move. This library is used by the games API, and you need it creating your own custom games. Ths source code is available in this repository.

### Backend Models

This backend models library **CNinnnovation.Codebraker.BackendModels** contains models for the Codebreaker app. Reference this library when creating a Codebreaker service, or use it from data access libraries.

### Database Libraries

This database libraries **CNinnnovation.Codebraker.Cosmos** and **CNinnnovation.Codebraker.SqlServer** contain EF Core contexts for the Codebreaker app. Reference one or both of these libraries storing games to the database.

### Client Library

The library **CNinnovation.Codbreaker.Client** is used by client applications to access the game APIs.

## Codebreaker Packages Feeds

[CNinnovation Codebreaker packages on NuGet](https://www.nuget.org/packages?q=cninnovation.codebreaker)

[Codebreaker Packages daily builds on Azure DevOps](https://pkgs.dev.azure.com/cnilearn/codebreakerpackages/_packaging/codebreaker/nuget/v3/index.json)

## Builds

### Libraries

#### Preview versions

|Branch|Analyzers|Backend models|Cosmos|SQL Server|Client Library|
|:--:|:--:|:--:|:--:|:--:|:--:|
**main**|[![Analyzers](https://github.com/CodebreakerApp/Codebreaker.Backend/actions/workflows/codebreaker-lib-analyzers.yml/badge.svg)](https://github.com/CodebreakerApp/Codebreaker.Backend/actions/workflows/codebreaker-lib-analyzers.yml)|[![Backend models](https://github.com/CodebreakerApp/Codebreaker.Backend/actions/workflows/codebreaker-lib-backendmodels.yml/badge.svg)](https://github.com/CodebreakerApp/Codebreaker.Backend/actions/workflows/codebreaker-lib-backendmodels.yml)|[![Cosmos](https://github.com/CodebreakerApp/Codebreaker.Backend/actions/workflows/codebreaker-lib-cosmos.yml/badge.svg)](https://github.com/CodebreakerApp/Codebreaker.Backend/actions/workflows/codebreaker-lib-cosmos.yml)|[![SQL Server](https://github.com/CodebreakerApp/Codebreaker.Backend/actions/workflows/codebreaker-lib-sqlserver.yml/badge.svg)](https://github.com/CodebreakerApp/Codebreaker.Backend/actions/workflows/codebreaker-lib-sqlserver.yml)|[![Client Library](https://github.com/CodebreakerApp/Codebreaker.Backend/actions/workflows/codebreaker-lib-client.yml/badge.svg)](https://github.com/CodebreakerApp/Codebreaker.Backend/actions/workflows/codebreaker-lib-client.yml)

#### Released versions

|Branch|Analyzers|Backend models|Cosmos|SQL Server|Client Library|
|:--:|:--:|:--:|:--:|:--:|:--:|
**main**|[![Analyzers](https://github.com/CodebreakerApp/Codebreaker.Backend/actions/workflows/codebreaker-lib-analyzers-stable.yml/badge.svg)](https://github.com/CodebreakerApp/Codebreaker.Backend/actions/workflows/codebreaker-lib-analyzers-stable.yml)|[![Backend models](https://github.com/CodebreakerApp/Codebreaker.Backend/actions/workflows/codebreaker-lib-backendmodels-stable.yml/badge.svg)](https://github.com/CodebreakerApp/Codebreaker.Backend/actions/workflows/codebreaker-lib-backendmodels-stable.yml)|[![Cosmos](https://github.com/CodebreakerApp/Codebreaker.Backend/actions/workflows/codebreaker-lib-cosmos-stable.yml/badge.svg)](https://github.com/CodebreakerApp/Codebreaker.Backend/actions/workflows/codebreaker-lib-cosmos-stable.yml)|[![SQL Server](https://github.com/CodebreakerApp/Codebreaker.Backend/actions/workflows/codebreaker-lib-sqlserver-stable.yml/badge.svg)](https://github.com/CodebreakerApp/Codebreaker.Backend/actions/workflows/codebreaker-lib-sqlserver-stable.yml)|[![Client Library](https://github.com/CodebreakerApp/Codebreaker.Backend/actions/workflows/codebreaker-lib-client-stable.yml/badge.svg)](https://github.com/CodebreakerApp/Codebreaker.Backend/actions/workflows/codebreaker-lib-client-stable.yml)

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

## Codebreaker Services

* REST API to play games, writes information to Cosmos
* Bot who plays games calling the API. The bot can be invoked calling commands from a REST API
* REST API for reporting
* SignalR Services to show live games

## Codebreaker Client apps

These apps are using .NET 7 with the previous version of the API. See the new clients in the new client repositories!

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
* Azure Storage message queue
* Azure Key Vault
