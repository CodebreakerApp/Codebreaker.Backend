# Codebreaker Backend

Codebreaker Backend is a .NET 9 microservices solution using .NET Aspire orchestration for a game platform. The solution includes game APIs, clients, analyzers, and multiple service components with Azure integrations.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Prerequisites
- Install .NET 9 SDK: `curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0`
- Install .NET 8 runtime for multi-targeted tests: `curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0 --runtime dotnet`
- Set PATH: `export PATH="$HOME/.dotnet:$PATH"`
- Verify installation: `dotnet --version` should show 9.0.103 or later

### Build Individual Solutions
The repository contains multiple solution files that can be built independently:

- `dotnet build src/Codebreaker.Analyzers.sln` -- takes 9 seconds. NEVER CANCEL. Set timeout to 20+ seconds.
- `dotnet build src/Codebreaker.Backend.Models.sln` -- takes 7 seconds. NEVER CANCEL. Set timeout to 20+ seconds.
- `dotnet build src/Codebreaker.Backend.slnx` -- main solution
- `dotnet build src/Codebreaker.GameAPIs.Client.sln` -- takes 10 seconds. NEVER CANCEL. Set timeout to 20+ seconds.
- `dotnet build src/Codebreaker.Backend.Cosmos.sln` -- library solution
- `dotnet build src/Codebreaker.Backend.SqlServer.sln` -- library solution
- `dotnet build src/Codebreaker.Backend.Postgres.sln` -- library solution

### Test Individual Solutions
- `dotnet test src/Codebreaker.Analyzers.sln` -- takes 4 seconds, runs 53 tests. NEVER CANCEL. Set timeout to 15+ seconds.
- `dotnet test src/Codebreaker.Backend.Models.sln` -- requires .NET 8 runtime for net8.0 tests
- `dotnet test src/Codebreaker.GameAPIs.Client.sln` -- takes 8 seconds, runs 11 tests. NEVER CANCEL. Set timeout to 20+ seconds.

### Build Individual Projects
Individual projects build faster than full solutions:
- `dotnet build src/services/gameapis/Codebreaker.GameAPIs/Codebreaker.GameAPIs.csproj` -- takes 3.5 seconds

### Run Individual Services
Services can be run independently for development:
- Game APIs: `cd src/services/gameapis/Codebreaker.GameAPIs && dotnet run` -- runs on http://localhost:9400
- Services start quickly but require proper database configuration for full functionality

### Full Solution Build Limitations
- The main solution `src/Codebreaker.Backend.slnx` is in .slnx format
- Building the AppHost project requires private Azure DevOps feeds: `dotnet build src/services/host/Codebreaker.AppHost/Codebreaker.AppHost.csproj` -- takes 45+ minutes and requires Azure DevOps authentication. NEVER CANCEL. Set timeout to 60+ minutes.
- Azure deployment requires `azd` CLI and Azure authentication

## Central Package Management
The repository uses Central Package Management with `src/Directory.Packages.props`. When adding PackageReference items:
- Do NOT specify versions in .csproj files
- Add package versions to `src/Directory.Packages.props`
- Build will fail if PackageReference has version but package not in Directory.Packages.props
- Common packages already configured: Microsoft.NET.Test.Sdk, xunit, Moq, coverlet.collector

## Validation
- Always run `dotnet format --verify-no-changes src/[solution].sln` before committing -- takes 10 seconds with detailed formatting analysis. NEVER CANCEL. Set timeout to 30+ seconds.
- Always build and test changes on individual solutions before attempting full stack
- Individual Game APIs service can be tested by running on localhost:9400
- Test health endpoint: `curl http://localhost:9400/health` (expects 500 error without database)
- Test Swagger: `curl http://localhost:9400/swagger` (expects 301 redirect)

## Architecture Overview
**Key Projects:**
- **AppHost**: .NET Aspire orchestration host (src/services/host/Codebreaker.AppHost)
- **Game APIs**: Core game service (src/services/gameapis/Codebreaker.GameAPIs)
- **Analyzers**: Game logic analyzers (src/services/common/Codebreaker.GameAPIs.Analyzers)
- **Client Libraries**: API clients (src/clients/Codebreaker.GameAPIs.Client)
- **Blazor App**: Web UI (src/services/Codebreaker.Blazor)
- **Gateway**: API Gateway with YARP (src/services/gateway)
- **Live Service**: SignalR service (src/services/live)
- **Ranking Service**: Game ranking (src/services/ranking)
- **User Service**: User management (src/services/user)
- **Bot Services**: Game bots (src/services/bot)

**Data Access:**
- Cosmos DB library: src/services/common/Codebreaker.Data.Cosmos
- SQL Server library: src/services/common/Codebreaker.Data.SqlServer
- PostgreSQL library: src/services/common/Codebreaker.Data.Postgres

## Azure Services Integration
The solution integrates with:
- Azure Container Apps
- Azure Cosmos DB
- Azure Active Directory B2C
- Azure SignalR Services
- Azure App Configuration
- Azure Event Hub
- Azure Storage
- Azure Key Vault

## Common Tasks
Use these outputs instead of running commands to save time:

### Repository Structure
```
src/
├── Codebreaker.*.sln         # Individual solution files
├── Codebreaker.Backend.slnx   # Main solution (.slnx format)
├── Directory.Build.props      # Common build properties
├── Directory.Packages.props   # Central package management
├── clients/                   # Client libraries
├── services/
│   ├── bot/                  # Game bot services
│   ├── common/               # Shared libraries (analyzers, data, models)
│   ├── gameapis/             # Core game service
│   ├── gateway/              # API gateway
│   ├── host/                 # Aspire AppHost
│   ├── live/                 # SignalR service
│   ├── ranking/              # Ranking service
│   ├── user/                 # User service
│   └── Codebreaker.Blazor/   # Web UI
```

### Target Frameworks
- Primary: net9.0
- Multi-targeting: net8.0;net9.0 (for libraries)
- Test projects: primarily net9.0

### Known Build Issues
- Central Package Management requires proper setup
- Private Azure DevOps feeds with anonymous access needed for full solution
- Some test projects may need package reference fixes
- Docker/container builds require Azure authentication

### Development Workflow
1. Make code changes to individual projects
2. Build individual solutions: `dotnet build src/[specific].sln`
3. Run tests: `dotnet test src/[specific].sln`
4. Format code: `dotnet format src/[specific].sln`
5. Test individual services with `dotnet run`
6. Use AppHost for full stack development (requires Azure setup)

### Performance Expectations
- Individual solution builds: 3-10 seconds
- Individual solution tests: 4-8 seconds
- Code formatting: 10 seconds
- Full AppHost build: 5+ minutes (requires private feeds)
- Service startup: 2-5 seconds