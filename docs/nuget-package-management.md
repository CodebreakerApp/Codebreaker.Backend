# NuGet Package Management: Preview and Stable Builds

This document outlines the best practices for managing preview and stable NuGet package builds in the Codebreaker Backend solution, including GitHub Actions workflows and deployment strategies.

## Overview

The Codebreaker Backend solution uses a dual-pipeline approach for NuGet package management:

- **Preview Builds**: Automatically triggered on code changes, deployed to Azure DevOps Artifacts
- **Stable Builds**: Manually triggered, deployed to both Azure DevOps Artifacts and public NuGet Gallery

## Architecture

```
Code Changes (main branch)
    ↓
Preview Build Pipeline
    ↓ (automatic)
Azure DevOps Artifacts (preview)
    ↓ (manual approval)
Stable Build Pipeline
    ↓ (automatic)
Azure DevOps Artifacts (stable) + NuGet Gallery
```

## Package Versioning Strategy

### Preview Builds
- **Format**: `{base-version}-preview.1.{build-number + offset}`
- **Example**: `3.8.0-preview.1.15` (if build number is 5 and offset is 10)
- **Trigger**: Automatic on push to main branch with changes in specific paths
- **Retention**: 3 days

### Stable Builds  
- **Format**: `{base-version}` (no suffix)
- **Example**: `3.8.0`
- **Trigger**: Manual workflow dispatch
- **Retention**: 30 days

## Central Package Management

The solution uses Central Package Management with `Directory.Packages.props`:

```xml
<PropertyGroup>
  <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  <CentralPackageTransitivePinningEnabled>false</CentralPackageTransitivePinningEnabled>
</PropertyGroup>
```

### Key Points:
- ✅ **DO**: Add package versions to `src/Directory.Packages.props`
- ❌ **DON'T**: Specify versions in individual `.csproj` files
- ⚠️ **Warning**: Build will fail if PackageReference has version but package not in Directory.Packages.props

## GitHub Actions Workflows

### 1. Preview Build Workflows

Each library has a dedicated preview workflow (e.g., `codebreaker-lib-sqlserver.yml`):

```yaml
name: Sql Server data lib

on:
  push:
    branches: [ main ]
    paths:
    - 'src/services/common/Codebreaker.Data.SqlServer/**'
  workflow_dispatch:

jobs:
  build:
    uses: CodebreakerApp/Codebreaker.Backend/.github/workflows/createnuget-withbuildnumber.yml@main
    with:
      version-suffix: preview.1.
      version-number: ${{ github.run_number }}
      version-offset: 10
      solutionfile-path: src/Codebreaker.Backend.SqlServer.slnx
      projectfile-path: src/services/common/Codebreaker.Data.SqlServer/Codebreaker.Data.SqlServer.csproj
      dotnet-version: '9.0.x'
      artifact-name: codebreaker-sqlserver
      branch-name: main

  publishdevops:
    uses: CodebreakerApp/Codebreaker.Backend/.github/workflows/publishnuget-azuredevops.yml@main
    needs: build
    with:
      artifact-name: codebreaker-sqlserver
    secrets: inherit
```

### 2. Stable Build Workflows

Each library has a stable workflow (e.g., `codebreaker-lib-sqlserver-stable.yml`):

```yaml
name: SqlServer stable lib

on:
  workflow_dispatch:

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout to the branch
        uses: actions/checkout@v5
        with:
          ref: main

      - name: Setup .NET
        uses: actions/setup-dotnet@v5
        with: 
          dotnet-version: 9.0.x

      - name: Build the library
        run: dotnet build -c Release ${{ env.solutionfile-path }}

      - name: Run the unit tests
        run: dotnet test ${{ env.solutionfile-path }}
        
      - name: Create a Package
        run: dotnet pack -c Release ${{ env.projectfile-path }} -o packages

  publishdevops:
    uses: CodebreakerApp/Codebreaker.Backend/.github/workflows/publishnuget-azuredevops.yml@main
    needs: build
    with:
      artifact-name: codebreaker-sqlserver-stable
    secrets: inherit

  publishnuget:
    uses: CodebreakerApp/Codebreaker.Backend/.github/workflows/publishnuget-nugetserver.yml@main
    needs: publishdevops
    with:
      artifact-name: codebreaker-sqlserver-stable
    secrets: inherit
```

### 3. Reusable Workflows

#### Build Workflow (`createnuget-withbuildnumber.yml`)

Features:
- **Deterministic builds** for reproducible packages
- **Source linking** with embedded sources  
- **Symbol packages** (.snupkg) generation
- **Version calculation** with offsets
- **Multi-targeting** support (net8.0;net9.0)

Key parameters:
```yaml
inputs:
  version-suffix: preview.1.     # Quality marker
  version-number: ${{ github.run_number }}  # Build number
  version-offset: 10             # Offset for version numbering
  solutionfile-path: src/Codebreaker.Backend.SqlServer.slnx
  projectfile-path: src/services/common/Codebreaker.Data.SqlServer/Codebreaker.Data.SqlServer.csproj
  dotnet-version: '9.0.x'
  artifact-name: codebreaker-sqlserver
```

Deterministic build settings:
```bash
/p:ContinuousIntegrationBuild=true
/p:Deterministic=true
/p:EmbedUntrackedSources=true
/p:DebugType=embedded
/p:PublishRepositoryUrl=true
/p:PathMap='$(MSBuildProjectDirectory)=/'
```

#### Azure DevOps Publish Workflow (`publishnuget-azuredevops.yml`)

```yaml
env:
  ARTIFACTS_URL: "https://pkgs.dev.azure.com/cnilearn/codebreakerpackages/_packaging/codebreaker/nuget/v3/index.json"

steps:
  - name: Add the Azure DevOps Artifacts Package Source
    run: dotnet nuget add source --username USERNAME --password ${{ secrets.DEVOPSARTIFACT_PAT }} --store-password-in-clear-text --name devopscninnovation ${{ env.ARTIFACTS_URL }}
      
  - name: Publish to Azure DevOps Artifacts
    run: dotnet nuget push "packages/*.nupkg" --api-key ${{ secrets.DEVOPSARTIFACT_PAT }} --source devopscninnovation --skip-duplicate
```

#### NuGet Gallery Publish Workflow (`publishnuget-nugetserver.yml`)

```yaml
env:
  ARTIFACTS_URL: https://api.nuget.org/v3/index.json

steps:
  - name: Publish to the NuGet server (nupkg and snupkg)
    run: dotnet nuget push "packages/*.nupkg" --api-key ${{ secrets.NUGETAPIKEY }} --source ${{ env.ARTIFACTS_URL }}
```

## Deployment Targets

### Azure DevOps Artifacts
- **Purpose**: Internal package repository for preview and stable builds
- **URL**: `https://pkgs.dev.azure.com/cnilearn/codebreakerpackages/_packaging/codebreaker/nuget/v3/index.json`
- **Authentication**: Personal Access Token (PAT)
- **Environment**: `DevOpsArtifacts`
- **Features**: 
  - Supports both preview and stable packages
  - Symbol packages (.snupkg)
  - Package retention policies

### NuGet Gallery
- **Purpose**: Public package repository for stable builds only
- **URL**: `https://api.nuget.org/v3/index.json`
- **Authentication**: API Key
- **Environment**: `NugetServer`
- **Features**:
  - Global package distribution
  - Automatic symbol package handling
  - Package signing validation

## Required Secrets

Configure these secrets in your GitHub repository:

| Secret Name | Environment | Description |
|-------------|-------------|-------------|
| `DEVOPSARTIFACT_PAT` | DevOpsArtifacts | Personal Access Token for Azure DevOps Artifacts |
| `NUGETAPIKEY` | NugetServer | API Key for NuGet Gallery |

## Best Practices

### 1. Version Management
- Always update the base version in `.csproj` files before creating stable releases
- Use semantic versioning (Major.Minor.Patch)
- Preview versions automatically increment with build numbers

### 2. Package Dependencies
- Keep internal package versions synchronized in `Directory.Packages.props`
- Use conditional package versions for multi-targeting:
  ```xml
  <PackageVersion Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.11" Condition="'$(TargetFramework)' == 'net8.0'" />
  <PackageVersion Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.9" Condition="'$(TargetFramework)' == 'net9.0'" />
  ```

### 3. Testing Strategy
- All builds run unit tests before packaging
- Preview builds enable faster feedback cycles
- Stable builds have additional validation through Azure DevOps Artifacts before NuGet Gallery

### 4. Path-Based Triggers
- Preview workflows use path filters to trigger only on relevant changes:
  ```yaml
  paths:
  - 'src/services/common/Codebreaker.Data.SqlServer/**'
  ```

### 5. Artifact Management
- Preview artifacts: 3-day retention
- Stable artifacts: 30-day retention  
- Unique artifact names prevent conflicts

## Workflow Execution Guide

### Triggering Preview Builds
1. **Automatic**: Push changes to main branch in monitored paths
2. **Manual**: Use "Run workflow" button on GitHub Actions tab

### Triggering Stable Builds
1. Navigate to GitHub Actions → Select stable workflow (e.g., "SqlServer stable lib")
2. Click "Run workflow" button
3. Confirm deployment to production environments

### Monitoring Builds
- Check GitHub Actions tab for build status
- Review Azure DevOps Artifacts for package availability
- Verify NuGet Gallery for stable package publication

## Troubleshooting

### Common Issues

#### Build Failures
- **Deterministic build issues**: Ensure all source files are committed
- **Package reference errors**: Verify `Directory.Packages.props` contains all required packages
- **Multi-targeting issues**: Check conditional package references

#### Deployment Failures
- **Azure DevOps authentication**: Verify PAT token permissions
- **NuGet Gallery publishing**: Check API key validity and package metadata
- **Duplicate packages**: Use `--skip-duplicate` flag (already configured)

#### Version Conflicts
- **Preview version conflicts**: Increase version offset in workflow
- **Stable version conflicts**: Update base version in project file

### Debug Commands

```bash
# Local package creation (preview)
dotnet pack --version-suffix preview.1.123 -c Release src/services/common/Codebreaker.Data.SqlServer/Codebreaker.Data.SqlServer.csproj

# Local package creation (stable)
dotnet pack -c Release src/services/common/Codebreaker.Data.SqlServer/Codebreaker.Data.SqlServer.csproj

# Test package installation
dotnet add package CNinnovation.Codebreaker.SqlServer --version 3.8.0-preview.1.123 --source https://pkgs.dev.azure.com/...
```

## Integration with Development Workflow

### For Library Developers
1. **Development**: Work on feature branches
2. **Integration**: Merge to main branch → Automatic preview build
3. **Testing**: Use preview packages in dependent projects
4. **Release**: Trigger stable build when ready for production

### For Library Consumers
1. **Development**: Use preview packages from Azure DevOps Artifacts
2. **Production**: Use stable packages from NuGet Gallery
3. **Testing**: Pin specific preview versions for reproducible builds

## Future Enhancements

### Planned Improvements
- **Automated release notes** generation from Git history
- **Package vulnerability scanning** integration
- **Performance regression testing** for library updates
- **Automated dependency updates** with Dependabot
- **Package usage analytics** and deprecation warnings

### Environment-Specific Deployments
- **Development Environment**: Auto-deploy preview packages
- **Staging Environment**: Manual promotion of stable candidates
- **Production Environment**: Stable packages only with approval gates

This documentation provides a comprehensive guide for managing NuGet packages in the Codebreaker Backend solution, ensuring reliable and efficient package distribution for both development and production scenarios.