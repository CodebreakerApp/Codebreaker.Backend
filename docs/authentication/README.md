# Authentication Documentation

This directory contains comprehensive documentation for authentication and authorization in the Codebreaker platform.

## Documentation Files

### [Microsoft External ID Configuration Guide](./microsoft-external-id.md)

Comprehensive guide covering:
- Gateway configuration with Microsoft External ID
- Token flow architecture between Gateway and APIs
- Blazor Server and Blazor WebAssembly client configuration
- Desktop client configuration (WPF, .NET MAUI, Uno Platform, WinUI)
- Security best practices
- Troubleshooting common issues

**When to use**: Reference this guide for detailed implementation instructions and comprehensive coverage of all authentication scenarios.

### [Quick Start Guide](./quick-start.md)

Fast-track guide with:
- 5-minute setup instructions
- Common configuration patterns
- Code snippets for each platform
- Quick troubleshooting tips

**When to use**: Use this guide when you need to quickly set up authentication or find a specific code snippet.

## Overview

The Codebreaker platform uses **Microsoft External ID** (formerly Azure AD B2C) for identity and access management across all client applications and backend services.

### Architecture

```
┌──────────────┐         ┌──────────────┐         ┌──────────────┐
│   Clients    │         │   Gateway    │         │  Backend     │
│  (Various)   │────────▶│   (YARP)     │────────▶│  Services    │
└──────────────┘         └──────────────┘         └──────────────┘
   • Blazor                • JWT Validation         • Game APIs
   • WPF                   • Token Forwarding       • Ranking
   • MAUI                  • Authorization          • Live
   • Uno Platform          • API Routing            • User Service
   • WinUI
```

### Key Components

1. **Gateway (YARP Reverse Proxy)**
   - Entry point for all API requests
   - JWT token validation
   - Authorization policy enforcement
   - Token forwarding to backend services

2. **Backend Services**
   - Game APIs, Ranking, Live, User services
   - Can optionally validate tokens (defense in depth)
   - Extract user claims for business logic

3. **Client Applications**
   - Multiple platforms supported
   - MSAL for desktop/mobile authentication
   - Microsoft.Identity.Web for Blazor
   - Automatic token management

## Getting Started

### For New Developers

1. Read the [Quick Start Guide](./quick-start.md) first
2. Set up your development environment
3. Configure authentication for your specific platform
4. Test with the provided code snippets

### For Detailed Implementation

1. Review the [comprehensive guide](./microsoft-external-id.md)
2. Understand the token flow architecture
3. Follow platform-specific configuration sections
4. Implement security best practices

## Platform Support Matrix

| Platform | Authentication Library | Status | Documentation |
|----------|----------------------|--------|---------------|
| Gateway (YARP) | Microsoft.Identity.Web | ✅ Configured | [Section](./microsoft-external-id.md#gateway-configuration) |
| Game APIs | Microsoft.Identity.Web | ⚠️ Optional | [Section](./microsoft-external-id.md#game-apis-service-configuration) |
| Blazor Server | Microsoft.Identity.Web | ✅ Supported | [Section](./microsoft-external-id.md#blazor-server-configuration) |
| Blazor WASM | MSAL.js | ✅ Supported | [Section](./microsoft-external-id.md#blazor-webassembly-wasm-configuration) |
| WPF | MSAL.NET | ✅ Supported | [Section](./microsoft-external-id.md#wpf-configuration) |
| .NET MAUI | MSAL.NET | ✅ Supported | [Section](./microsoft-external-id.md#net-maui-configuration) |
| Uno Platform | MSAL.NET | ✅ Supported | [Section](./microsoft-external-id.md#uno-platform-configuration) |
| WinUI 3 | MSAL.NET | ✅ Supported | [Section](./microsoft-external-id.md#winui-3-configuration) |

## Configuration Overview

### Minimum Required Configuration

All platforms require:

1. **Azure AD B2C Tenant**: Microsoft Entra ID with External ID configured
2. **App Registration**: One per platform/client type
3. **Redirect URIs**: Platform-specific callback URLs
4. **Client ID**: Application (client) ID from app registration
5. **Authority URL**: B2C tenant and policy endpoint

### Environment Variables

Development:
```bash
AzureAdB2C__Instance=https://your-tenant.b2clogin.com
AzureAdB2C__Domain=your-tenant.onmicrosoft.com
AzureAdB2C__ClientId=<client-id>
AzureAdB2C__SignUpSignInPolicyId=B2C_1_SUSI
```

Production (use Azure Key Vault):
```bash
az keyvault secret set --vault-name gateway-keyvault \
  --name "AzureAdB2C--ClientSecret" \
  --value "<client-secret>"
```

## Common Scenarios

### Scenario 1: Web Application (Blazor)

**Use Case**: Public-facing web application with user authentication

**Setup**:
- Blazor Server or Blazor WASM
- Microsoft External ID for authentication
- Gateway forwards authenticated requests to APIs

**Documentation**: [Blazor Client Configuration](./microsoft-external-id.md#blazor-client-configuration)

### Scenario 2: Desktop Application (WPF/MAUI)

**Use Case**: Native desktop/mobile application

**Setup**:
- MSAL.NET for authentication
- Native platform authentication UI
- Direct API calls through Gateway

**Documentation**: [Desktop Client Configuration](./microsoft-external-id.md#desktop-client-configuration)

### Scenario 3: Anonymous + Authenticated Users

**Use Case**: Game supporting both guest and registered players

**Setup**:
- Anonymous user service for guest accounts
- Optional authentication upgrade
- Mixed authorization policies

**Documentation**: 
- [Identity Service README](../../src/services/identity/README.md)
- [Gateway Configuration](./microsoft-external-id.md#gateway-configuration)

### Scenario 4: API-to-API Communication

**Use Case**: Backend service calling another backend service

**Setup**:
- Client credentials flow
- Service principal authentication
- Managed identity

**Documentation**: [Token Flow Architecture](./microsoft-external-id.md#token-flow-architecture)

## Security Considerations

### Critical Security Practices

1. **Never commit secrets**: Use Azure Key Vault or user secrets
2. **Use HTTPS in production**: HTTP only for local development
3. **Validate tokens at multiple layers**: Gateway AND API services
4. **Implement proper CORS**: Don't use `AllowAnyOrigin()` in production
5. **Rotate secrets regularly**: Set up automated secret rotation
6. **Monitor authentication**: Log and alert on suspicious activity

See [Security Best Practices](./microsoft-external-id.md#security-best-practices) for detailed guidance.

## Troubleshooting

### Quick Diagnostics

```bash
# Check if token is valid
curl -H "Authorization: Bearer <token>" https://your-gateway-url.com/games

# Decode JWT token
# Visit https://jwt.ms and paste your token

# Test authentication endpoint
curl https://your-tenant.b2clogin.com/your-tenant.onmicrosoft.com/B2C_1_SUSI/v2.0/.well-known/openid-configuration
```

### Common Issues

1. **CORS errors**: See [Troubleshooting](./microsoft-external-id.md#2-cors-errors-in-browser)
2. **Token validation failures**: See [Troubleshooting](./microsoft-external-id.md#1-the-access-token-provided-is-not-valid-error)
3. **Redirect URI mismatch**: See [Troubleshooting](./microsoft-external-id.md#6-redirect-uri-mismatch)
4. **Authentication state not persisting**: See [Troubleshooting](./microsoft-external-id.md#8-blazor-wasm-authentication-state-not-persisting)

Full troubleshooting guide: [Troubleshooting Section](./microsoft-external-id.md#troubleshooting)

## Additional Resources

### Official Microsoft Documentation

- [Microsoft Identity Platform](https://learn.microsoft.com/en-us/azure/active-directory/develop/)
- [Microsoft External ID](https://learn.microsoft.com/en-us/azure/active-directory-b2c/)
- [MSAL.NET](https://learn.microsoft.com/en-us/azure/active-directory/develop/msal-overview)
- [Microsoft.Identity.Web](https://learn.microsoft.com/en-us/azure/active-directory/develop/microsoft-identity-web)

### Codebreaker Resources

- [Identity Service](../../src/services/identity/)
- [Gateway Service](../../src/services/gateway/)
- [Game APIs](../../src/services/gameapis/)
- [Main README](../../README.md)

### Sample Code

- [Microsoft Identity Samples](https://github.com/Azure-Samples?q=active-directory)
- [Codebreaker Backend Repository](https://github.com/CodebreakerApp/Codebreaker.Backend)

## Contributing

Found an issue or have a suggestion? 

1. Check existing [issues](https://github.com/CodebreakerApp/Codebreaker.Backend/issues)
2. Open a new issue with details
3. Submit a pull request with improvements

## Support

For questions or issues:

- **Documentation issues**: Open a GitHub issue
- **Implementation questions**: Check [Troubleshooting](./microsoft-external-id.md#troubleshooting)
- **Microsoft Identity issues**: See [Official Documentation](https://learn.microsoft.com/en-us/azure/active-directory/)

## License

This documentation is part of the Codebreaker project. See [LICENSE](../../LICENSE) for details.
