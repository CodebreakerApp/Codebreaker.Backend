# Microsoft Entra External ID Architecture Diagrams

This document provides visual representations of the authentication flows in the Codebreaker platform using Microsoft Entra External ID.

## High-Level Architecture

```mermaid
graph TB
    subgraph "Codebreaker Platform"
        subgraph "Client Apps"
            Blazor[Blazor Web App]
            WPF[WPF Desktop]
            MAUI[.NET MAUI]
            Uno[Uno Platform]
            WinUI[WinUI 3]
        end
        
        subgraph "Gateway Layer"
            Gateway[Gateway YARP<br/>• JWT Validation<br/>• Authorization<br/>• Routing]
        end
        
        subgraph "Backend Services"
            GameAPI[Game APIs]
            Ranking[Ranking Service]
            Live[Live Service]
            User[User Service]
        end
        
        subgraph "External Identity"
            ExternalID[Microsoft Entra<br/>External ID]
        end
        
        Blazor -->|JWT Token| Gateway
        WPF -->|JWT Token| Gateway
        MAUI -->|JWT Token| Gateway
        Uno -->|JWT Token| Gateway
        WinUI -->|JWT Token| Gateway
        
        Gateway -->|JWT Forward| GameAPI
        Gateway -->|JWT Forward| Ranking
        Gateway -->|JWT Forward| Live
        Gateway -->|JWT Forward| User
        
        Blazor -.->|Authentication| ExternalID
        WPF -.->|Authentication| ExternalID
        MAUI -.->|Authentication| ExternalID
        Uno -.->|Authentication| ExternalID
        WinUI -.->|Authentication| ExternalID
    end
```

## Authentication Flow (Web - Blazor)

### Blazor Server

```mermaid
sequenceDiagram
    participant Client as Client Browser
    participant Blazor as Blazor Server
    participant ExternalID as Microsoft Entra External ID
    participant Gateway as Gateway (YARP)

    Client->>Blazor: 1. Visit App
    Blazor->>Client: 2. Redirect to External ID Login
    Client->>ExternalID: 3. User Authenticates
    ExternalID->>Client: 4. Return Auth Code
    Client->>Blazor: 5. Submit Code
    Blazor->>ExternalID: 6. Exchange Code for Tokens
    ExternalID->>Blazor: 7. Return Tokens (ID + Access)
    Blazor->>Client: 8. Set Authentication Cookie
    
    Client->>Blazor: 9. API Request with Cookie
    Blazor->>Gateway: 10. API Call with JWT Token
    Gateway->>Gateway: 11. Validate JWT
    Gateway->>Blazor: 12. Response
    Blazor->>Client: 13. Response
```

### Blazor WebAssembly

```mermaid
sequenceDiagram
    participant Client as Client Browser
    participant WASM as Blazor WASM (MSAL.js)
    participant ExternalID as Microsoft Entra External ID
    participant Gateway as Gateway (YARP)

    Client->>WASM: 1. Load App
    WASM->>WASM: 2. Check Authentication State
    WASM->>ExternalID: 3. Redirect to External ID Login
    ExternalID->>Client: 4. User Login & Consent
    ExternalID->>WASM: 5. Return Token in URL Fragment
    WASM->>WASM: 6. Process & Store Token (SessionStorage)
    
    Client->>Gateway: 7. API Request with Bearer Token
    Gateway->>Gateway: 8. Validate JWT
    Gateway->>Client: 9. Response
```

## Authentication Flow (Desktop - Native Apps)

### WPF, WinUI, MAUI, Uno Platform

```mermaid
sequenceDiagram
    participant App as Native App
    participant MSAL as MSAL.NET
    participant ExternalID as Microsoft Entra External ID
    participant Gateway as Gateway (YARP)

    App->>MSAL: 1. User Clicks Login
    MSAL->>MSAL: 2. Check Token Cache
    
    alt Token Cache Empty
        MSAL->>ExternalID: 3. Launch Browser for Login
        App->>App: 4. Show Browser Window
        ExternalID->>MSAL: 5. User Authenticates
        ExternalID->>MSAL: 6. Return Auth Code
        MSAL->>ExternalID: 7. Exchange Code for Token
        ExternalID->>MSAL: 8. Return Access Token
        MSAL->>MSAL: 9. Cache Token (Encrypted)
    end
    
    MSAL->>App: 10. Login Success
    
    App->>MSAL: 11. Request API Call
    MSAL->>MSAL: 12. Get Token from Cache
    App->>Gateway: 13. API Request with Bearer Token
    Gateway->>Gateway: 14. Validate JWT
    Gateway->>App: 15. Response
    App->>App: 16. Display Result
```

## Token Flow Through Gateway

```mermaid
flowchart TD
    Client[Client App] -->|API Request + JWT| Gateway[Gateway YARP]
    
    subgraph Gateway Processing
        Validate[Validate JWT Token<br/>• Verify Signature<br/>• Check Expiration<br/>• Validate Issuer<br/>• Validate Audience]
        Authorize[Apply Authorization Policy<br/>• Check Required Scopes<br/>• Check Required Roles<br/>• Check Custom Claims]
        Route[Route to Backend Service]
    end
    
    Gateway --> Validate
    Validate --> Authorize
    Authorize --> Route
    
    Route -->|Forward Request + JWT| GameAPI[Game API Service]
    Route -->|Forward Request + JWT| Ranking[Ranking Service]
    Route -->|Forward Request + JWT| Live[Live Service]
    Route -->|Forward Request + JWT| User[User Service]
    
    subgraph Backend Processing
        ExtractClaims[Extract Claims<br/>• User ID<br/>• Email<br/>• Name<br/>• Custom Claims]
        ProcessRequest[Process Business Logic]
    end
    
    GameAPI --> ExtractClaims
    ExtractClaims --> ProcessRequest
    ProcessRequest --> Gateway
    Gateway --> Client
```

## Configuration Components

```mermaid
graph TB
    subgraph "Microsoft Entra External ID Tenant"
        UserFlow[User Flows<br/>• Sign Up<br/>• Sign In<br/>• Profile Management]
        
        subgraph "App Registrations"
            GatewayApp[Gateway App<br/>• Client ID<br/>• Redirect URIs<br/>• API Permissions]
            BlazorApp[Blazor App<br/>• Client ID<br/>• Redirect URIs<br/>• Scopes]
            DesktopApp[Desktop Apps<br/>• Client ID<br/>• Redirect URIs<br/>• Scopes]
        end
        
        APIScopes[API Scopes<br/>• Games.Play<br/>• Games.Query<br/>• Games.Admin<br/>• Bot.Play]
    end
    
    subgraph "Application Configuration"
        subgraph "Gateway Config"
            GatewaySettings[appsettings.json<br/>• Instance<br/>• Domain<br/>• TenantId<br/>• ClientId]
            GatewayCode[Program.cs<br/>• AddAuthentication<br/>• AddMicrosoftIdentityWebApi<br/>• AddAuthorization<br/>• AddReverseProxy]
        end
        
        subgraph "Key Vault"
            Secrets[Secrets<br/>• EntraExternalId--ClientSecret]
        end
        
        subgraph "Backend Services"
            GameAPIs[Game APIs<br/>Optional JWT Auth]
            RankingService[Ranking Service<br/>Optional JWT Auth]
            LiveService[Live Service<br/>Optional JWT Auth]
            UserService[User Service<br/>Required JWT Auth]
        end
    end
    
    GatewayApp --> GatewaySettings
    BlazorApp --> GatewaySettings
    DesktopApp --> GatewaySettings
    APIScopes --> GatewaySettings
    Secrets --> GatewayCode
```

## Security Boundaries

```mermaid
graph TB
    subgraph "Public Internet (Untrusted)"
        BlazorWASM[Blazor WASM]
        WPFApp[WPF App]
        MAUIApp[MAUI App]
    end
    
    subgraph "DMZ / Edge (First Line of Defense)"
        subgraph "Gateway Security"
            JWTValidation[JWT Validation]
            Authorization[Authorization Policies]
            RateLimit[Rate Limiting]
            CORS[CORS Configuration]
        end
        Gateway[Gateway YARP]
    end
    
    subgraph "Internal Network (Trusted)"
        GameAPI[Game APIs]
        Ranking[Ranking Service]
        Live[Live Service]
        User[User Service]
    end
    
    subgraph "Data Layer (Most Trusted)"
        Cosmos[Cosmos DB]
        Redis[Redis Cache]
        EventHub[Event Hub]
        Storage[Azure Storage]
    end
    
    BlazorWASM -->|HTTPS + JWT| Gateway
    WPFApp -->|HTTPS + JWT| Gateway
    MAUIApp -->|HTTPS + JWT| Gateway
    
    Gateway --> JWTValidation
    JWTValidation --> Authorization
    Authorization --> RateLimit
    RateLimit --> CORS
    
    Gateway -->|Internal Network + JWT| GameAPI
    Gateway -->|Internal Network + JWT| Ranking
    Gateway -->|Internal Network + JWT| Live
    Gateway -->|Internal Network + JWT| User
    
    GameAPI --> Cosmos
    GameAPI --> Redis
    GameAPI --> EventHub
    Ranking --> EventHub
    Live --> EventHub
    User --> Storage
```

## Token Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Login: User Initiates Login
    
    Login --> TokenIssuance: Authenticate with External ID
    TokenIssuance --> TokenStorage: Store Tokens Securely
    
    state TokenIssuance {
        [*] --> IDToken: ID Token (User Identity)
        [*] --> AccessToken: Access Token (API Authorization)
        [*] --> RefreshToken: Refresh Token (Long-lived)
    }
    
    state TokenStorage {
        [*] --> WebStorage: Web: HttpOnly Cookies/SessionStorage
        [*] --> DesktopStorage: Desktop: Encrypted Cache (DPAPI/Keychain)
    }
    
    TokenStorage --> TokenUsage: Attach to API Requests
    
    state TokenUsage {
        [*] --> RequestHeader: Authorization: Bearer <access_token>
        RequestHeader --> GatewayValidation: Gateway Validates Token
        
        state GatewayValidation {
            [*] --> VerifySignature: Verify Signature (Public Key)
            VerifySignature --> CheckExpiration: Check Expiration (nbf, exp)
            CheckExpiration --> ValidateIssuer: Validate Issuer (iss)
            ValidateIssuer --> ValidateAudience: Validate Audience (aud)
        }
        
        GatewayValidation --> ProcessRequest: Forward to Backend
    }
    
    TokenUsage --> TokenRefresh: Before Expiration
    
    state TokenRefresh {
        [*] --> SilentAcquisition: Try Silent Token Acquisition
        SilentAcquisition --> UseRefreshToken: Use Refresh Token if Needed
        UseRefreshToken --> ReAuthenticate: Re-authenticate if Refresh Fails
    }
    
    TokenRefresh --> TokenUsage: Continue with New Token
    
    TokenUsage --> TokenRevocation: User Logout
    
    state TokenRevocation {
        [*] --> ClearCache: Remove from Client Cache
        ClearCache --> ClearSession: Clear Session
        ClearSession --> SignOutEndpoint: Redirect to Sign-out Endpoint
    }
    
    TokenRevocation --> TokenExpiration: Token Becomes Invalid
    
    state TokenExpiration {
        [*] --> NaturalExpiration: Natural Expiration (1 hour)
        [*] --> ManualRevocation: Manual Revocation
        [*] --> SignOut: User Sign-out
    }
    
    TokenExpiration --> [*]
```

## Comparison: Azure AD B2C vs Microsoft Entra External ID

```mermaid
graph LR
    subgraph "Service Comparison"
        subgraph "Azure AD B2C (Legacy)"
            B2CEndpoints[Endpoints: *.b2clogin.com]
            B2CAuthority[Authority: https://tenant.b2clogin.com/<br/>tenant.onmicrosoft.com/B2C_1_SUSI]
            B2CConfig[Configuration:<br/>• Instance<br/>• Domain<br/>• ClientId<br/>• SignUpSignInPolicyId]
            B2CFeatures[Features:<br/>• Custom Policies<br/>• Complex Scenarios<br/>• Mature/Stable]
        end
        
        subgraph "Microsoft Entra External ID (Current)"
            ExternalIDEndpoints[Endpoints: *.ciamlogin.com]
            ExternalIDAuthority[Authority: https://tenant.ciamlogin.com/<br/>tenant-id]
            ExternalIDConfig[Configuration:<br/>• Instance<br/>• Domain<br/>• TenantId<br/>• ClientId]
            ExternalIDFeatures[Features:<br/>• Simplified Setup<br/>• Modern APIs<br/>• Streamlined UX]
        end
    end
    
    B2CEndpoints -.->|Migration| ExternalIDEndpoints
    B2CAuthority -.->|Migration| ExternalIDAuthority
    B2CConfig -.->|Migration| ExternalIDConfig
    B2CFeatures -.->|Migration| ExternalIDFeatures
```

## Platform-Specific Configuration Flow

```mermaid
flowchart TD
    Start[Start Implementation] --> ChoosePlatform{Choose Platform}
    
    ChoosePlatform -->|Web| BlazorSetup[Blazor Setup]
    ChoosePlatform -->|Desktop| DesktopSetup[Desktop Setup]
    
    subgraph "Blazor Configuration"
        BlazorSetup --> BlazorType{Blazor Type?}
        BlazorType -->|Server| BlazorServer[Blazor Server<br/>• Microsoft.Identity.Web<br/>• OpenIdConnect<br/>• Server-side cookies]
        BlazorType -->|WASM| BlazorWASM[Blazor WASM<br/>• MSAL.js<br/>• Browser-based auth<br/>• SessionStorage tokens]
    end
    
    subgraph "Desktop Configuration"
        DesktopSetup --> DesktopType{Desktop Type?}
        DesktopType -->|WPF| WPFConfig[WPF<br/>• MSAL.NET<br/>• Windows Authentication<br/>• DPAPI token cache]
        DesktopType -->|MAUI| MAUIConfig[.NET MAUI<br/>• MSAL.NET<br/>• Cross-platform<br/>• Platform-specific storage]
        DesktopType -->|Uno| UnoConfig[Uno Platform<br/>• MSAL.NET<br/>• Multi-platform<br/>• WebAssembly support]
        DesktopType -->|WinUI| WinUIConfig[WinUI 3<br/>• MSAL.NET<br/>• Windows 11 optimized<br/>• Modern Windows UI]
    end
    
    BlazorServer --> ConfigureAuth[Configure Authentication]
    BlazorWASM --> ConfigureAuth
    WPFConfig --> ConfigureAuth
    MAUIConfig --> ConfigureAuth
    UnoConfig --> ConfigureAuth
    WinUIConfig --> ConfigureAuth
    
    ConfigureAuth --> TestImplementation[Test Implementation]
    TestImplementation --> Production[Deploy to Production]
```


## See Also

- [Microsoft Entra External ID Configuration Guide](./microsoft-external-id.md)
- [Quick Start Guide](./quick-start.md)
- [Authentication Overview](./README.md)
- [Azure AD B2C Migration Guide](./azure-ad-b2c.md)
