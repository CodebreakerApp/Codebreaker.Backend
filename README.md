# Codebreaker Backends

This is the GitHub repo for the backend services of the **Codebreaker** solution.

See these repositories for clients:

* [WinUI, .NET MAUI, WPF, Uno Platform](https://github.com/codebreakerapp/Codebreaker.Xaml)
* [Blazor](https://github.com/codebreakerapp/Codebreaker.Blazor)

See this repository for the book covering the backend solution:

[Pragmatic Microservices with C# and Azure](https://github.com/PacktPublishing/Pragmatic-Microservices-with-CSharp-and-Azure/)

## Version 3

This repository has been updated with the source code from the book, but is updated and enhanced continuously.

### Games API

The new Games API service with access to SQL Server and Azure Cosmos DB. Currently, you need to run this locally. A hosted version will be available at a later date.

### Analyzers Library

The analyzers library **CNinnovation.Codebreaker.Analyzers** is used in the backend to return results based on a game move. This library is used by the games API, and you need it creating your own custom games. Ths source code is available in this repository.

NuGet: https://www.nuget.org/packages/CNinnovation.Codebreaker.Analyzers

### Backend Models

This backend models library **CNinnnovation.Codebraker.BackendModels** contains models for the Codebreaker app. Reference this library when creating a Codebreaker service, or use it from data access libraries.

NuGet: https://www.nuget.org/packages/CNinnovation.Codebreaker.BackendModels

### Database Libraries

This database libraries **CNinnnovation.Codebraker.Cosmos** and **CNinnnovation.Codebraker.SqlServer** contain EF Core contexts for the Codebreaker app. Reference one or both of these libraries storing games to the database.

NuGet Cosmos: https://www.nuget.org/packages/CNinnovation.Codebreaker.Cosmos

NuGet SQL Server: https://www.nuget.org/packages/CNinnovation.Codebreaker.SqlServer

### Client Library

The library **CNinnovation.Codbreaker.Client** is used by client applications to access the game APIs.

NuGet: https://www.nuget.org/packages/CNinnovation.Codebreaker.GamesClient

## Codebreaker Package Feeds

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

### Integration Tests

TODO: this test will move to this repo

|Branch|Game API|
|:--:|:--:|
**main**|[![Test Game API Integration](https://github.com/CNinnovation/codebreaker/actions/workflows/codebreakerapi-integrationtests.yml/badge.svg)](https://github.com/CNinnovation/codebreaker/actions/workflows/codebreakerapi-integrationtests.yml)

## Guidelines

[Guidelines](guidelines.md)

## Codebreaker Services

* Gateway - a YARP service acting as reverse proxy
* Games API - the games API using Azure Cosmos DB and Redis for a game cache, and using Azure Event Hub for notification on completed games
* Live service - using SignalR to monitor completed games, using the Event Hub to receive information on completed games, and offers SignalR to subscribe to this information
* Ranking - using Event Hub to receive completed games, offering a REST API to see ranking information

## Codebreaker Client apps

* [Blazor](https://blazor.codebreaker.app)

> Register an account and play the games!

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

## Authentication Configuration

The Codebreaker platform uses **Microsoft External ID** (formerly Azure AD B2C) for authentication across all components.

### Documentation

- **[Complete Authentication Guide](docs/authentication/microsoft-external-id.md)**: Comprehensive documentation covering Gateway, APIs, Blazor, WPF, MAUI, Uno Platform, and WinUI configuration
- **[Quick Start Guide](docs/authentication/quick-start.md)**: Fast-track setup with code snippets and common patterns
- **[Authentication Overview](docs/authentication/README.md)**: Architecture overview and platform support matrix

### Quick Setup

For the Gateway service with Azure AD B2C:

1. Configure `appsettings.json`:
```json
{
  "AzureAdB2C": {
    "Instance": "https://<tenant>.b2clogin.com",
    "Domain": "<tenant>.onmicrosoft.com",
    "ClientId": "<client-id>",
    "SignUpSignInPolicyId": "B2C_1_SUSI"
  }
}
```

2. The API connectors need to be updated after publishing the backend:
   - Set Endpoint URLs to `https://<gateway>/users/api-connectors/validate-before-user-creation` and `https://<gateway>/users/api-connectors/enrich-token`
   - Store the basic-authentication password in `gateway-keyvault` with the key `AADB2C-ApiConnector-Password`
   - Set that password in the API connectors configuration

For detailed configuration instructions for all platforms, see the [authentication documentation](docs/authentication/).
