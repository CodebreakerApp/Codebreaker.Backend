# Microsoft External ID Architecture Diagrams

This document provides visual representations of the authentication flows in the Codebreaker platform.

## High-Level Architecture

```
┌────────────────────────────────────────────────────────────────────────┐
│                         Codebreaker Platform                            │
│                                                                         │
│  ┌─────────────────┐        ┌─────────────────┐       ┌──────────────┐│
│  │  Client Apps    │        │     Gateway     │       │   Backend    ││
│  │                 │        │     (YARP)      │       │   Services   ││
│  │  • Blazor       │───────▶│                 │──────▶│              ││
│  │  • WPF          │  JWT   │  • JWT Validate │  JWT  │  • Game APIs ││
│  │  • MAUI         │  Token │  • Authorize    │  Fwd  │  • Ranking   ││
│  │  • Uno Platform │        │  • Route        │       │  • Live      ││
│  │  • WinUI        │        │                 │       │  • User      ││
│  └─────────────────┘        └─────────────────┘       └──────────────┘│
│           ▲                                                            │
│           │ Token                                                      │
│           │                                                            │
│  ┌────────┴─────────┐                                                 │
│  │ Microsoft        │                                                  │
│  │ External ID      │                                                  │
│  │ (Azure AD B2C)   │                                                  │
│  └──────────────────┘                                                  │
└────────────────────────────────────────────────────────────────────────┘
```

## Authentication Flow (Web - Blazor)

### Blazor Server

```
┌──────┐         ┌─────────┐        ┌──────────┐       ┌─────────┐
│Client│         │ Blazor  │        │ External │       │ Gateway │
│Browser         │ Server  │        │    ID    │       │  (YARP) │
└──┬───┘         └────┬────┘        └────┬─────┘       └────┬────┘
   │                  │                  │                   │
   │ 1. Visit App     │                  │                   │
   ├─────────────────▶│                  │                   │
   │                  │                  │                   │
   │ 2. Redirect to   │                  │                   │
   │    Login         │                  │                   │
   │◀─────────────────┤                  │                   │
   │                  │                  │                   │
   │ 3. Authenticate  │                  │                   │
   ├─────────────────────────────────────▶│                   │
   │                  │                  │                   │
   │ 4. Return Auth   │                  │                   │
   │    Code          │                  │                   │
   │◀─────────────────────────────────────┤                   │
   │                  │                  │                   │
   │ 5. Submit Code   │                  │                   │
   ├─────────────────▶│                  │                   │
   │                  │ 6. Exchange Code │                   │
   │                  │    for Tokens    │                   │
   │                  ├─────────────────▶│                   │
   │                  │                  │                   │
   │                  │ 7. Return Tokens │                   │
   │                  │   (ID + Access)  │                   │
   │                  │◀─────────────────┤                   │
   │                  │                  │                   │
   │ 8. Set Cookie    │                  │                   │
   │◀─────────────────┤                  │                   │
   │                  │                  │                   │
   │ 9. API Request   │                  │                   │
   │    with Cookie   │                  │                   │
   ├─────────────────▶│                  │                   │
   │                  │ 10. API Call     │                   │
   │                  │     with Token   │                   │
   │                  ├───────────────────────────────────────▶│
   │                  │                  │                   │
   │                  │ 11. Validate JWT │                   │
   │                  │                  │                   │
   │                  │ 12. Response     │                   │
   │                  │◀───────────────────────────────────────┤
   │                  │                  │                   │
   │ 13. Response     │                  │                   │
   │◀─────────────────┤                  │                   │
```

### Blazor WebAssembly

```
┌──────┐         ┌─────────┐        ┌──────────┐       ┌─────────┐
│Client│         │ WASM    │        │ External │       │ Gateway │
│Browser         │ (MSAL.js)        │    ID    │       │  (YARP) │
└──┬───┘         └────┬────┘        └────┬─────┘       └────┬────┘
   │                  │                  │                   │
   │ 1. Load App      │                  │                   │
   ├─────────────────▶│                  │                   │
   │                  │                  │                   │
   │ 2. Check Auth    │                  │                   │
   │                  │                  │                   │
   │ 3. Redirect to   │                  │                   │
   │    B2C Login     │                  │                   │
   ├────────────────────────────────────▶│                   │
   │                  │                  │                   │
   │ 4. User Login    │                  │                   │
   │                  │                  │                   │
   │ 5. Return Token  │                  │                   │
   │    in URL        │                  │                   │
   │◀────────────────────────────────────┤                   │
   │                  │                  │                   │
   │ 6. Process Token │                  │                   │
   │◀────────────────▶│                  │                   │
   │                  │                  │                   │
   │ 7. Store Token   │                  │                   │
   │    (SessionStorage)                 │                   │
   │                  │                  │                   │
   │ 8. API Request   │                  │                   │
   │    Authorization:│                  │                   │
   │    Bearer <token>│                  │                   │
   ├─────────────────────────────────────────────────────────▶│
   │                  │                  │ 9. Validate JWT   │
   │                  │                  │                   │
   │                  │                  │ 10. Response      │
   │◀─────────────────────────────────────────────────────────┤
```

## Authentication Flow (Desktop - Native Apps)

### WPF, WinUI, MAUI, Uno Platform

```
┌──────────┐         ┌─────────┐        ┌──────────┐       ┌─────────┐
│  Native  │         │  MSAL   │        │ External │       │ Gateway │
│   App    │         │  .NET   │        │    ID    │       │  (YARP) │
└────┬─────┘         └────┬────┘        └────┬─────┘       └────┬────┘
     │                    │                  │                   │
     │ 1. User clicks     │                  │                   │
     │    Login           │                  │                   │
     ├───────────────────▶│                  │                   │
     │                    │ 2. Check Cache   │                   │
     │                    │                  │                   │
     │                    │ 3. If no token,  │                   │
     │                    │    launch browser│                   │
     │                    ├─────────────────▶│                   │
     │                    │                  │                   │
     │ 4. Show Browser    │                  │                   │
     │    for Login       │                  │                   │
     │◀───────────────────┤                  │                   │
     │                    │                  │                   │
     │ 5. User Authenticates                 │                   │
     │                    │                  │                   │
     │                    │ 6. Return Auth   │                   │
     │                    │    Code          │                   │
     │                    │◀─────────────────┤                   │
     │                    │                  │                   │
     │                    │ 7. Exchange Code │                   │
     │                    │    for Token     │                   │
     │                    ├─────────────────▶│                   │
     │                    │                  │                   │
     │                    │ 8. Return Token  │                   │
     │                    │◀─────────────────┤                   │
     │                    │                  │                   │
     │                    │ 9. Cache Token   │                   │
     │                    │    (Encrypted)   │                   │
     │                    │                  │                   │
     │ 10. Login Success  │                  │                   │
     │◀───────────────────┤                  │                   │
     │                    │                  │                   │
     │ 11. API Call       │                  │                   │
     ├───────────────────▶│                  │                   │
     │                    │ 12. Get Token    │                   │
     │                    │     from Cache   │                   │
     │                    │                  │                   │
     │                    │ 13. API Request  │                   │
     │                    │     Bearer Token │                   │
     ├───────────────────────────────────────────────────────────▶│
     │                    │                  │                   │
     │                    │                  │ 14. Validate JWT  │
     │                    │                  │                   │
     │                    │                  │ 15. Response      │
     │◀───────────────────────────────────────────────────────────┤
     │                    │                  │                   │
     │ 16. Display Result │                  │                   │
```

## Token Flow Through Gateway

```
┌─────────┐         ┌──────────────────────────────────────┐         ┌──────────┐
│ Client  │         │         Gateway (YARP)               │         │  Game    │
│   App   │         │                                      │         │   API    │
└────┬────┘         └──────────────────────────────────────┘         └────┬─────┘
     │                                                                      │
     │ 1. Request with                                                     │
     │    Authorization: Bearer <JWT>                                      │
     ├─────────────────────▶│                                              │
     │                      │                                              │
     │                      │ 2. Validate JWT Token                        │
     │                      │    • Verify signature                        │
     │                      │    • Check expiration                        │
     │                      │    • Validate issuer                         │
     │                      │    • Validate audience                       │
     │                      │                                              │
     │                      │ 3. Apply Authorization Policy                │
     │                      │    • Check required scopes                   │
     │                      │    • Check required roles                    │
     │                      │    • Check custom claims                     │
     │                      │                                              │
     │                      │ 4. If authorized, forward request            │
     │                      │    with original Authorization header        │
     │                      ├─────────────────────────────────────────────▶│
     │                      │                                              │
     │                      │                      5. (Optional) Re-validate
     │                      │                         JWT at API level     │
     │                      │                                              │
     │                      │                      6. Extract claims        │
     │                      │                         • User ID            │
     │                      │                         • Email              │
     │                      │                         • Name               │
     │                      │                                              │
     │                      │                      7. Process request      │
     │                      │                                              │
     │                      │ 8. Return Response                           │
     │                      │◀─────────────────────────────────────────────┤
     │                      │                                              │
     │ 9. Response          │                                              │
     │◀─────────────────────┤                                              │
```

## Configuration Components

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        Microsoft External ID Tenant                      │
│                                                                          │
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐           │
│  │  User Flow     │  │   App Reg      │  │   App Reg      │           │
│  │  B2C_1_SUSI    │  │   (Gateway)    │  │   (Blazor)     │           │
│  │                │  │                │  │                │           │
│  │  • Sign Up     │  │  Client ID     │  │  Client ID     │           │
│  │  • Sign In     │  │  Redirect URIs │  │  Redirect URIs │           │
│  │  • Profile     │  │  API Scopes    │  │  Scopes        │           │
│  └────────────────┘  └────────────────┘  └────────────────┘           │
│                                                                          │
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐           │
│  │   App Reg      │  │   App Reg      │  │  API Scopes    │           │
│  │   (WPF)        │  │   (MAUI)       │  │                │           │
│  │                │  │                │  │  • Games.Play  │           │
│  │  Client ID     │  │  Client ID     │  │  • Games.Query │           │
│  │  Redirect URIs │  │  Redirect URIs │  │  • Games.Admin │           │
│  │  Scopes        │  │  Scopes        │  │  • Bot.Play    │           │
│  └────────────────┘  └────────────────┘  └────────────────┘           │
└─────────────────────────────────────────────────────────────────────────┘
                                ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                         Application Layer                                │
│                                                                          │
│  ┌─────────────────────────────────────────────────────────────────────┐│
│  │                           Gateway                                    ││
│  │                                                                      ││
│  │  appsettings.json                Program.cs                         ││
│  │  • Instance                      • AddAuthentication()               ││
│  │  • Domain                        • AddMicrosoftIdentityWebApi()      ││
│  │  • ClientId                      • AddAuthorization()                ││
│  │  • SignUpSignInPolicyId          • AddReverseProxy()                 ││
│  │                                                                      ││
│  │  Key Vault: gateway-keyvault                                        ││
│  │  • AADB2C-ApiConnector-Password                                     ││
│  └─────────────────────────────────────────────────────────────────────┘│
│                                                                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  │
│  │  Game APIs  │  │  Ranking    │  │    Live     │  │    User     │  │
│  │             │  │             │  │             │  │             │  │
│  │  Optional:  │  │  Optional:  │  │  Optional:  │  │  Required:  │  │
│  │  JWT Auth   │  │  JWT Auth   │  │  JWT Auth   │  │  JWT Auth   │  │
│  └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘  │
└─────────────────────────────────────────────────────────────────────────┘
```

## Security Boundaries

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          Trust Boundaries                                │
│                                                                          │
│  ┌────────────────────────────────────────────┐                         │
│  │        Public Internet (Untrusted)          │                         │
│  │                                            │                         │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐ │                         │
│  │  │  Blazor  │  │   WPF    │  │  MAUI    │ │                         │
│  │  │   WASM   │  │   App    │  │   App    │ │                         │
│  │  └──────────┘  └──────────┘  └──────────┘ │                         │
│  └────────────────────────────────────────────┘                         │
│                       │ HTTPS + JWT                                      │
│                       ▼                                                  │
│  ┌────────────────────────────────────────────┐                         │
│  │    DMZ / Edge (First Line of Defense)      │                         │
│  │                                            │                         │
│  │         ┌─────────────────┐                │                         │
│  │         │    Gateway      │                │                         │
│  │         │    (YARP)       │                │                         │
│  │         │                 │                │                         │
│  │         │  • JWT Validate │                │                         │
│  │         │  • Authorize    │                │                         │
│  │         │  • Rate Limit   │                │                         │
│  │         │  • CORS         │                │                         │
│  │         └─────────────────┘                │                         │
│  └────────────────────────────────────────────┘                         │
│                       │ Internal Network + JWT                           │
│                       ▼                                                  │
│  ┌────────────────────────────────────────────┐                         │
│  │  Internal Services (Trusted Network)       │                         │
│  │                                            │                         │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐ │                         │
│  │  │  Game    │  │ Ranking  │  │   Live   │ │                         │
│  │  │  APIs    │  │ Service  │  │ Service  │ │                         │
│  │  └──────────┘  └──────────┘  └──────────┘ │                         │
│  │       │             │             │        │                         │
│  └───────┼─────────────┼─────────────┼────────┘                         │
│          │             │             │                                   │
│          ▼             ▼             ▼                                   │
│  ┌────────────────────────────────────────────┐                         │
│  │       Data Layer (Most Trusted)             │                         │
│  │                                            │                         │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐ │                         │
│  │  │  Cosmos  │  │  Redis   │  │ EventHub │ │                         │
│  │  │    DB    │  │  Cache   │  │          │ │                         │
│  │  └──────────┘  └──────────┘  └──────────┘ │                         │
│  └────────────────────────────────────────────┘                         │
└─────────────────────────────────────────────────────────────────────────┘
```

## Token Lifecycle

```
┌──────────────────────────────────────────────────────────────────┐
│                      Token Lifecycle                              │
│                                                                   │
│  ┌─────────────┐                                                 │
│  │  1. Login   │  User authenticates with External ID            │
│  └──────┬──────┘                                                 │
│         │                                                        │
│         ▼                                                        │
│  ┌─────────────┐                                                 │
│  │  2. Token   │  Receive ID Token + Access Token               │
│  │  Issuance   │  • ID Token: User identity claims              │
│  └──────┬──────┘  • Access Token: API authorization             │
│         │         • Refresh Token: Long-lived                   │
│         │                                                        │
│         ▼                                                        │
│  ┌─────────────┐                                                 │
│  │  3. Token   │  Client stores tokens securely                 │
│  │  Storage    │  • Web: HttpOnly cookies / SessionStorage     │
│  └──────┬──────┘  • Desktop: Encrypted cache (DPAPI, Keychain) │
│         │                                                        │
│         ▼                                                        │
│  ┌─────────────┐                                                 │
│  │  4. Token   │  Token attached to API requests                │
│  │  Usage      │  Authorization: Bearer <access_token>          │
│  └──────┬──────┘                                                 │
│         │                                                        │
│         ▼                                                        │
│  ┌─────────────┐                                                 │
│  │  5. Token   │  Gateway/API validates:                        │
│  │  Validation │  • Signature (using public key)                │
│  └──────┬──────┘  • Expiration (nbf, exp claims)               │
│         │         • Issuer (iss claim)                          │
│         │         • Audience (aud claim)                        │
│         ▼                                                        │
│  ┌─────────────┐                                                 │
│  │  6. Token   │  Before expiration:                            │
│  │  Refresh    │  • Try silent acquisition                      │
│  └──────┬──────┘  • Use refresh token if needed                │
│         │         • Re-authenticate if refresh fails            │
│         │                                                        │
│         ▼                                                        │
│  ┌─────────────┐                                                 │
│  │  7. Token   │  On logout:                                    │
│  │  Revocation │  • Remove from client cache                    │
│  └──────┬──────┘  • Clear session                               │
│         │         • Redirect to sign-out endpoint               │
│         │                                                        │
│         ▼                                                        │
│  ┌─────────────┐                                                 │
│  │  8. Token   │  Token becomes invalid                         │
│  │  Expiration │  • Natural expiration (typically 1 hour)       │
│  └─────────────┘  • Manual revocation                           │
│                   • Sign-out                                    │
└──────────────────────────────────────────────────────────────────┘
```

## See Also

- [Microsoft External ID Configuration Guide](./microsoft-external-id.md)
- [Quick Start Guide](./quick-start.md)
- [Authentication Overview](./README.md)
