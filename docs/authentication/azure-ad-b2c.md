# Azure AD B2C Migration Guide

This guide provides information for migrating from Azure AD B2C to Microsoft Entra External ID in the Codebreaker platform.

> **Note**: This document serves as a migration reference. The Codebreaker project is transitioning to **Microsoft Entra External ID** as the primary authentication solution. For new implementations, see the [Microsoft Entra External ID Configuration Guide](./microsoft-external-id.md).

## Overview

Azure AD B2C was the predecessor to Microsoft Entra External ID. While Azure AD B2C remains supported, Microsoft Entra External ID provides a more modern and streamlined approach to external identity management.

### Why Migrate?

- **Simplified Configuration**: No complex policy management
- **Modern API Surface**: Better developer experience
- **Improved Performance**: Faster authentication flows
- **Future-Proof**: Active development and new features
- **Better Integration**: Seamless integration with other Microsoft services

## Key Differences

| Aspect | Azure AD B2C | Microsoft Entra External ID |
|--------|--------------|------------------------------|
| **Endpoints** | `*.b2clogin.com` | `*.ciamlogin.com` |
| **Authority Format** | `https://tenant.b2clogin.com/tenant.onmicrosoft.com/B2C_1_SUSI` | `https://tenant.ciamlogin.com/tenant-id` |
| **Configuration** | Policy-based (B2C_1_SUSI) | Tenant ID-based |
| **Setup Complexity** | Complex custom policies | Simplified user flows |
| **Portal** | Azure Portal | Microsoft Entra admin center |
| **API Endpoints** | Legacy Graph API | Modern Microsoft Graph |

## Migration Steps

### 1. Setup Microsoft Entra External ID

#### Create External ID Tenant

1. Navigate to [Microsoft Entra admin center](https://entra.microsoft.com)
2. Go to **External Identities** ? **External ID for customers**
3. Create a new External ID tenant
4. Configure user flows for sign-up and sign-in

#### Create App Registrations

Create new app registrations in the External ID tenant for:
- Gateway/API backend
- Blazor client
- WPF/MAUI desktop clients

### 2. Update Configuration

#### Gateway Configuration

**Before (Azure AD B2C):**
```json
{
  "AzureAdB2C": {
    "Instance": "https://codebreaker3000.b2clogin.com",
    "Domain": "codebreaker3000.onmicrosoft.com",
    "ClientId": "f528866c-c051-4e1e-8309-91831d52d8b5",
    "SignedOutCallbackPath": "/signout/B2C_1_SUSI",
    "SignUpSignInPolicyId": "B2C_1_SUSI"
  }
}
```

**After (Microsoft Entra External ID):**
```json
{
  "EntraExternalId": {
    "Instance": "https://codebreaker3000.ciamlogin.com",
    "Domain": "codebreaker3000.onmicrosoft.com",
    "TenantId": "12345678-1234-1234-1234-123456789012",
    "ClientId": "f528866c-c051-4e1e-8309-91831d52d8b5",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-callback-oidc"
  }
}
```

#### Program.cs Changes

**Before:**
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));
```

**After:**
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("EntraExternalId"));
```

#### Blazor WASM Configuration

**Before:**
```json
{
  "AzureAdB2C": {
    "Authority": "https://codebreakertest.b2clogin.com/codebreakertest.onmicrosoft.com/B2C_1_SUSI",
    "ClientId": "6f3b0b17-f116-4b56-a63f-74a16b02730a",
    "ValidateAuthority": false
  }
}
```

**After:**
```json
{
  "EntraExternalId": {
    "Authority": "https://codebreakertest.ciamlogin.com/12345678-1234-1234-1234-123456789012",
    "ClientId": "6f3b0b17-f116-4b56-a63f-74a16b02730a",
    "ValidateAuthority": true
  }
}
```

#### Desktop Client (WPF/MAUI) Changes

**Before:**
```csharp
var app = PublicClientApplicationBuilder
    .Create(clientId)
    .WithB2CAuthority("https://tenant.b2clogin.com/tfp/tenant.onmicrosoft.com/B2C_1_SUSI")
    .Build();
```

**After:**
```csharp
var app = PublicClientApplicationBuilder
    .Create(clientId)
    .WithAuthority("https://tenant.ciamlogin.com/tenant-id")
    .Build();
```

### 3. Update Redirect URIs

#### Web Applications

- **Old format**: Works with both systems
- **New format**: Optimized for External ID

#### Desktop Applications

- **WPF/WinUI**: No changes needed
- **MAUI**: May need updated redirect URI schemes

### 4. Test Migration

#### Validation Checklist

- [ ] **Authentication flows work**: Users can sign in/out
- [ ] **Token validation**: APIs accept new tokens
- [ ] **Claims mapping**: User data is preserved
- [ ] **Refresh tokens**: Silent token renewal works
- [ ] **Cross-platform**: All client types function
- [ ] **Error handling**: Graceful failure scenarios

#### Testing Strategy

1. **Setup parallel environments**: Keep B2C for fallback
2. **Test individual components**: Start with one client type
3. **Validate token compatibility**: Ensure APIs work with both
4. **Load testing**: Verify performance under load
5. **User acceptance testing**: Validate user experience

### 5. Rollout Strategy

#### Phased Approach

1. **Phase 1**: Update development environment
2. **Phase 2**: Migrate staging environment
3. **Phase 3**: Update client applications
4. **Phase 4**: Production cutover
5. **Phase 5**: Decommission Azure AD B2C

#### Rollback Plan

- Maintain Azure AD B2C configuration as backup
- Keep both configuration sections during transition
- Use feature flags to switch between providers
- Monitor error rates and user feedback

## Legacy Azure AD B2C Configuration Reference

### Gateway Configuration (Legacy)

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using idunno.Authentication.Basic;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddBasic("BasicScheme", options =>
    {
        options.Events = new BasicAuthenticationEvents()
        {
            OnValidateCredentials = context =>
            {
                var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var password = config["AADB2C-ApiConnector-Password"];

                if (context.Username == "AADB2C" && context.Password == password)
                {
                    // Basic auth for B2C API connectors
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.Name, context.Username),
                        new Claim(ClaimTypes.Role, "AzureActiveDirectoryB2C")
                    };
                    context.Principal = new(new ClaimsIdentity(claims, context.Scheme.Name));
                    context.Success();
                }
                return Task.CompletedTask;
            }
        };
    })
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("usersApiConnectorsPolicy", config =>
    {
        config.RequireAuthenticatedUser()
              .AddAuthenticationSchemes("BasicScheme")
              .RequireRole("AzureActiveDirectoryB2C");
    });
```

### Desktop Client Configuration (Legacy)

```csharp
// B2C specific authority format
var authority = "https://tenant.b2clogin.com/tfp/tenant.onmicrosoft.com/B2C_1_SUSI";

var app = PublicClientApplicationBuilder
    .Create(clientId)
    .WithB2CAuthority(authority)
    .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
    .Build();
```

## Migration Considerations

### Breaking Changes

1. **Authority URL format**: Different structure between B2C and External ID
2. **Token claims**: Some claim names may differ
3. **API connector authentication**: May not be available in External ID
4. **Custom policies**: Not supported in External ID
5. **Configuration sections**: Must update all config references

### Compatibility Notes

- **Microsoft.Identity.Web**: Same library supports both
- **MSAL.NET**: Same library supports both
- **Token validation**: Same validation logic works
- **Scopes**: Same scope format applies

### Performance Considerations

- **Token expiration**: May differ between services
- **Refresh token lifetime**: Check token policies
- **Rate limiting**: Different limits may apply
- **Geographic availability**: Verify region support

## Troubleshooting Migration

### Common Issues

#### 1. Authority Validation Failed

**Cause**: Using old B2C authority format

**Solution**:
```csharp
// Wrong - B2C format
.WithAuthority("https://tenant.b2clogin.com/tfp/tenant.onmicrosoft.com/B2C_1_SUSI")

// Correct - External ID format
.WithAuthority("https://tenant.ciamlogin.com/tenant-id")
```

#### 2. Token Validation Errors

**Cause**: Issuer claim mismatch

**Solution**: Update token validation parameters:
```csharp
options.TokenValidationParameters = new TokenValidationParameters
{
    ValidIssuers = new[]
    {
        "https://tenant.ciamlogin.com/tenant-id/v2.0", // External ID
        "https://tenant.b2clogin.com/tenant-id/v2.0/"  // B2C fallback during migration
    }
};
```

#### 3. Claims Missing

**Cause**: Different claim names between B2C and External ID

**Solution**: Map claims appropriately:
```csharp
services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters.NameClaimType = "name";
    options.TokenValidationParameters.RoleClaimType = "roles";
});
```

### Migration Testing

#### Unit Tests

```csharp
[Fact]
public async Task Should_Validate_ExternalId_Token()
{
    // Arrange
    var token = CreateExternalIdToken();
    
    // Act
    var result = await ValidateToken(token);
    
    // Assert
    Assert.True(result.IsValid);
    Assert.Equal(expectedUserId, result.Claims.FindFirst(ClaimTypes.NameIdentifier)?.Value);
}
```

#### Integration Tests

```csharp
[Fact]
public async Task Should_Authenticate_With_ExternalId()
{
    // Test end-to-end authentication flow
    var client = _factory.CreateClient();
    var response = await client.GetAsync("/games");
    
    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    
    // Add token and retry
    client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", externalIdToken);
    
    response = await client.GetAsync("/games");
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}
```

## Timeline and Deprecation

### Microsoft's Roadmap

- **Azure AD B2C**: Continues to be supported
- **Microsoft Entra External ID**: Active development
- **New features**: Primarily in External ID
- **Migration timeline**: No forced migration date announced

### Recommended Timeline

- **Q1**: Setup External ID environment, test basic flows
- **Q2**: Migrate development environment, update documentation
- **Q3**: Production migration, monitor performance
- **Q4**: Decommission Azure AD B2C infrastructure

## Support and Resources

### Documentation

- [Microsoft Entra External ID Configuration Guide](./microsoft-external-id.md)
- [Quick Start Guide](./quick-start.md)
- [Architecture Diagrams](./architecture-diagrams.md)

### Microsoft Resources

- [External ID Migration Guide](https://learn.microsoft.com/en-us/entra/external-id/)
- [Azure AD B2C to External ID Comparison](https://learn.microsoft.com/en-us/entra/external-id/customers/concept-supported-features-customers)
- [Migration Support](https://docs.microsoft.com/en-us/azure/active-directory-b2c/migrate-to-external-id)

### Community

- [GitHub Issues](https://github.com/CodebreakerApp/Codebreaker.Backend/issues)
- [Microsoft Q&A](https://docs.microsoft.com/en-us/answers/topics/azure-ad-b2c.html)
- [Stack Overflow](https://stackoverflow.com/questions/tagged/azure-ad-b2c+microsoft-entra-external-id)

## Contributing

Found an issue with the migration process or have suggestions?