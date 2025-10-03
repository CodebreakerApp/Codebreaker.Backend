# Codebreaker.Identity

A .NET 8 library for managing anonymous users in Codebreaker using Microsoft Graph.

## Features

- Create anonymous users with Microsoft Graph
- Delete inactive anonymous users (those who haven't logged in for at least 3 months)

## Getting Started

### Installation

```bash
dotnet add package Codebreaker.Identity
```

### Configuration

Register the anonymous user service in your dependency injection container:

```csharp
// Using predefined configuration section
builder.Services.Configure<AnonymousUserOptions>(
    builder.Configuration.GetSection("AnonymousUsers"));
builder.Services.AddAnonymousUserService();

// Or with inline configuration
builder.Services.AddAnonymousUserService(options =>
{
    options.TenantId = "your-tenant-id";
    options.ClientId = "your-client-id";
    options.ClientSecret = "your-client-secret";
    options.Domain = "yourdomain.com";
    options.UserNamePrefix = "anon";
    options.PasswordLength = 12;
});

// For testing
builder.Services.AddMockAnonymousUserService();
```

### Usage

```csharp
// Inject the service
private readonly IAnonymousUserService _anonymousUserService;

public YourClass(IAnonymousUserService anonymousUserService)
{
    _anonymousUserService = anonymousUserService;
}

// Create an anonymous user
public async Task<AnonymousUser> CreateAnonymousUser(string userName)
{
    return await _anonymousUserService.CreateAnonUser(userName);
}

// Delete inactive users
public async Task<int> CleanupStaleUsers()
{
    return await _anonymousUserService.DeleteAnonUsers();
}
```

## Configuration Options

| Option | Description | Default |
|--------|-------------|---------|
| TenantId | Azure AD tenant ID | |
| ClientId | Azure AD application client ID | |
| ClientSecret | Azure AD application client secret | |
| Domain | Domain for anonymous user emails | |
| UserNamePrefix | Prefix for anonymous usernames | "anon" |
| PasswordLength | Length of generated password | 12 |

## Tests

The library includes both unit tests and a `MockAnonymousUserService` implementation for testing purposes.