# Rido.BFLite Architecture

This document provides detailed architecture diagrams and explanations of the Rido.BFLite library structure, dependencies, and data flow.

## Table of Contents

- [Overview](#overview)
- [Component Architecture](#component-architecture)
- [Package Dependencies](#package-dependencies)
- [Class Relationships](#class-relationships)
- [Request Flow](#request-flow)
- [Teams Extension Architecture](#teams-extension-architecture)
- [Authentication Flow](#authentication-flow)

## Overview

Rido.BFLite is a lightweight library for building Microsoft Bot Framework bots with minimal overhead. It provides a streamlined API surface while maintaining full compatibility with the Bot Framework protocol.

## Component Architecture

The following diagram shows the high-level component structure and how they interact:

```mermaid
graph TB
    subgraph "Application Layer"
        App[Bot Application/Sample]
    end

    subgraph "Rido.BFLite.Teams Package"
        TBA[TeamsBotApplication]
        TA[TeamsActivity]
        TCD[TeamsChannelData]
        IUW[InstallationUpdateWrapper]
    end

    subgraph "Rido.BFLite.Core Package"
        BA[BotApplication]
        CC[ConversationClient]
        A[Activity]
        CD[ChannelData]
        MRA[MessageReactionActivityWrapper]
    end

    subgraph "Hosting Layer"
        ABE[AppBuilderExtensions]
        MSE[MessageLoopServiceCollectionExtensions]
        WAS[WebApiSecurity]
    end

    subgraph "External Dependencies"
        ASP[ASP.NET Core]
        MIW[Microsoft.Identity.Web]
        JWT[JWT Bearer Authentication]
        BF[Bot Framework Service]
    end

    App --> TBA
    App --> ABE
    App --> MSE
    
    TBA --> BA
    TBA --> TA
    TBA --> IUW
    
    TA --> A
    TCD --> CD
    
    BA --> CC
    BA --> A
    BA --> MRA
    
    ABE --> BA
    ABE --> ASP
    
    MSE --> BA
    MSE --> CC
    MSE --> ASP
    
    WAS --> JWT
    WAS --> MIW
    WAS --> ASP
    
    CC --> BF
    CC --> MIW
    
    style App fill:#e1f5ff
    style TBA fill:#fff4e1
    style BA fill:#ffe1f5
    style CC fill:#f5e1ff
    style ABE fill:#e1ffe1
```

## Package Dependencies

This diagram illustrates the dependency relationships between packages and external libraries:

```mermaid
graph LR
    subgraph "Samples"
        SEB[Samples.BotEcho]
    end

    subgraph "Library Packages"
        BFLT[Rido.BFLite.Teams]
        BFLC[Rido.BFLite.Core]
    end

    subgraph "Microsoft Dependencies"
        ASPNET[ASP.NET Core 9.0]
        MIW[Microsoft.Identity.Web]
        MIWDA[Microsoft.Identity.Web.DownstreamApi]
        JWT[Microsoft.AspNetCore.Authentication.JwtBearer]
        OIDC[Microsoft.AspNetCore.Authentication.OpenIdConnect]
    end

    subgraph "Bot Framework"
        BFAPI[Bot Framework Service API]
    end

    SEB -->|Project Reference| BFLT
    BFLT -->|Project Reference| BFLC
    
    BFLC -->|Package Reference| MIW
    BFLC -->|Package Reference| MIWDA
    BFLC -->|Package Reference| JWT
    BFLC -->|Package Reference| OIDC
    BFLC -->|SDK| ASPNET
    
    SEB -->|SDK| ASPNET
    BFLT -->|SDK| ASPNET
    
    BFLC -.->|HTTP Calls| BFAPI
    
    style SEB fill:#e1f5ff
    style BFLT fill:#fff4e1
    style BFLC fill:#ffe1f5
    style ASPNET fill:#e1ffe1
```

## Class Relationships

This diagram shows the detailed class relationships and inheritance hierarchy:

```mermaid
classDiagram
    class BotApplication {
        -ILogger logger
        -ConversationClient conversationClient
        +Activity LastActivity
        +OnNewActivity EventHandler
        +OnMessage Action~Activity~
        +OnMessageReaction Action~MessageReactionActivityWrapper~
        +ProcessAsync(HttpContext) Task~string~
        +SendActivityAsync(Activity) Task~string~
    }

    class TeamsBotApplication {
        +OnInstallationUpdate Action~InstallationUpdateWrapper~
    }

    class ConversationClient {
        -IHttpClientFactory httpClientFactory
        -IAuthorizationHeaderProvider tokenProvider
        -ILogger logger
        +SendActivityAsync(Activity, CancellationToken) Task~string~
    }

    class Activity~T~ {
        +string Type
        +string ChannelId
        +string Text
        +string Id
        +string ServiceUrl
        +string ReplyToId
        +T ChannelData
        +ConversationAccount From
        +ConversationAccount Recipient
        +Conversation Conversation
        +JsonArray Entities
        +ExtendedPropertiesDictionary Properties
        +CreateReplyActivity(string) Activity
        +ToJson() string
        +FromJsonString(string) Activity~T~
    }

    class Activity {
        <<inherits Activity~ChannelData~>>
    }

    class TeamsActivity {
        <<inherits Activity~TeamsChannelData~>>
        +TeamsConversationAccount From
        +TeamsConversationAccount Recipient
        +TeamsConversation Conversation
        +TeamsChannelData ChannelData
        +FromActivity(Activity) TeamsActivity$
    }

    class ChannelData {
        +string ClientActivityId
        +IDictionary~string,object~ Properties
    }

    class TeamsChannelData {
        +TeamsChannelDataSettings Settings
        +string TeamsChannelId
        +string TeamsTeamId
        +TeamsChannel Channel
        +Team Team
        +TeamsChannelDataTenant Tenant
    }

    class ConversationAccount {
        +string Id
        +string Name
        +string AadObjectId
        +string Role
    }

    class TeamsConversationAccount {
        +string UserPrincipalName
        +string Email
    }

    class Conversation {
        +string Id
        +ExtendedPropertiesDictionary Properties
    }

    class TeamsConversation {
        +string TenantId
        +string ConversationType
    }

    class MessageReactionActivityWrapper {
        +Activity Activity
        +IList~MessageReaction~ ReactionsAdded
        +IList~MessageReaction~ ReactionsRemoved
    }

    class InstallationUpdateWrapper {
        +TeamsActivity Activity
        +string Action
        +string SelectedChannelId
        +bool IsAdd
        +bool IsRemove
    }

    class AppBuilderExtensions {
        <<static>>
        +UseBotApplication~T~(IApplicationBuilder) T$
    }

    class MessageLoopServiceCollectionExtensions {
        <<static>>
        +AddMessageLoop~T~(IServiceCollection) IServiceCollection$
    }

    class WebApiSecurity {
        <<static>>
        +AddBotFrameworkAuthentication(IServiceCollection, string) void$
    }

    TeamsBotApplication --|> BotApplication
    BotApplication --> ConversationClient : uses
    BotApplication --> Activity : processes
    BotApplication --> MessageReactionActivityWrapper : creates
    
    Activity~T~ --> ChannelData : contains
    Activity --|> Activity~T~
    TeamsActivity --|> Activity~T~
    TeamsActivity --> TeamsChannelData : uses
    TeamsActivity --> TeamsConversation : contains
    TeamsActivity --> TeamsConversationAccount : contains
    
    TeamsChannelData --|> ChannelData
    TeamsConversation --|> Conversation
    TeamsConversationAccount --|> ConversationAccount
    
    Activity~T~ --> Conversation : contains
    Activity~T~ --> ConversationAccount : contains
    
    MessageReactionActivityWrapper --> Activity : wraps
    InstallationUpdateWrapper --> TeamsActivity : wraps
    TeamsBotApplication --> InstallationUpdateWrapper : creates
    
    ConversationClient --> Activity : sends
    AppBuilderExtensions --> BotApplication : configures
    MessageLoopServiceCollectionExtensions --> BotApplication : registers
    MessageLoopServiceCollectionExtensions --> ConversationClient : registers
```

## Request Flow

This sequence diagram illustrates how a message is processed from the Bot Framework Service to the bot application and back:

```mermaid
sequenceDiagram
    participant BFS as Bot Framework Service
    participant ASP as ASP.NET Core
    participant Auth as Authentication Middleware
    participant EP as /api/messages Endpoint
    participant BA as BotApplication
    participant Handler as OnMessage Handler
    participant CC as ConversationClient
    participant Token as Token Provider

    BFS->>ASP: POST /api/messages (Activity JSON + JWT)
    ASP->>Auth: Validate JWT Token
    Auth->>Auth: Validate Issuer (botframework.com)
    Auth->>Auth: Validate Audience (Bot App ID)
    Auth->>Auth: Validate Signature
    Auth->>EP: Authorized Request
    
    EP->>BA: ProcessAsync(HttpContext)
    BA->>BA: ParseActivityAsync()
    BA->>BA: Deserialize Activity JSON
    BA->>BA: Store LastActivity
    
    alt Message Activity
        BA->>BA: Raise OnNewActivity Event
        BA->>Handler: Invoke OnMessage(activity)
        Handler->>Handler: Process Message
        Handler->>Handler: CreateReplyActivity()
        Handler->>BA: SendActivityAsync(reply)
    else MessageReaction Activity
        BA->>BA: Create MessageReactionActivityWrapper
        BA->>Handler: Invoke OnMessageReaction(wrapper)
        Handler->>BA: SendActivityAsync(reply)
    else InstallationUpdate (Teams)
        BA->>BA: Create TeamsActivity
        BA->>BA: Create InstallationUpdateWrapper
        BA->>Handler: Invoke OnInstallationUpdate(wrapper)
    end
    
    BA->>CC: SendActivityAsync(activity)
    CC->>Token: CreateAuthorizationHeaderForAppAsync()
    Token->>Token: Acquire Token for Bot Framework
    Token-->>CC: Bearer Token
    CC->>CC: Prepare HTTP Request
    CC->>BFS: POST /v3/conversations/{id}/activities
    BFS-->>CC: HTTP 200 + Activity ID
    CC-->>BA: Activity ID
    BA-->>EP: Success Response
    EP-->>ASP: Response
    ASP-->>BFS: HTTP 200
```

## Teams Extension Architecture

This diagram shows how the Teams extension enhances the core library:

```mermaid
graph TB
    subgraph "Teams-Specific Features"
        TBA[TeamsBotApplication]
        TA[TeamsActivity]
        TCD[TeamsChannelData]
        TCA[TeamsConversationAccount]
        TC[TeamsConversation]
        IUW[InstallationUpdateWrapper]
        TCH[TeamsChannel]
        T[Team]
        TCDT[TeamsChannelDataTenant]
        TCDS[TeamsChannelDataSettings]
    end

    subgraph "Core Base Classes"
        BA[BotApplication]
        A[Activity]
        CD[ChannelData]
        CA[ConversationAccount]
        CO[Conversation]
    end

    TBA -->|extends| BA
    TBA -->|handles| IUW
    
    TA -->|inherits| A
    TA -->|uses| TCD
    TA -->|uses| TCA
    TA -->|uses| TC
    
    TCD -->|inherits| CD
    TCD -->|contains| TCH
    TCD -->|contains| T
    TCD -->|contains| TCDT
    TCD -->|contains| TCDS
    
    TCA -->|inherits| CA
    TC -->|inherits| CO
    
    IUW -->|wraps| TA
    IUW -->|extracts| TCD
    
    style TBA fill:#fff4e1
    style TA fill:#fff4e1
    style TCD fill:#fff4e1
    style TCA fill:#fff4e1
    style TC fill:#fff4e1
    style IUW fill:#fff4e1
    style BA fill:#ffe1f5
    style A fill:#ffe1f5
    style CD fill:#ffe1f5
```

## Authentication Flow

This diagram illustrates the JWT authentication process for Bot Framework requests:

```mermaid
sequenceDiagram
    participant BFS as Bot Framework Service
    participant MW as Auth Middleware
    participant Config as OpenID Config
    participant Keys as Signing Keys
    participant Policy as Authorization Policy

    BFS->>MW: POST /api/messages<br/>Authorization: Bearer {JWT}
    
    MW->>MW: Extract JWT Token
    MW->>Config: Fetch OpenID Configuration
    Note over Config: https://login.botframework.com/v1/<br/>.well-known/openid-configuration
    Config-->>MW: OIDC Configuration
    
    MW->>Keys: Fetch Signing Keys
    Keys-->>MW: Public Keys
    
    MW->>MW: Validate Token
    Note over MW: - Issuer: api.botframework.com<br/>- Audience: Bot App ID<br/>- Signature: Valid<br/>- Expiration: Not expired
    
    alt Token Valid
        MW->>MW: Create ClaimsPrincipal
        MW->>Policy: Check "Bot" Policy
        Policy->>Policy: Verify Claims<br/>- aud claim exists<br/>- aud == Bot App ID<br/>- User authenticated
        alt Policy Satisfied
            Policy-->>MW: Authorized
            MW->>BFS: Continue to Endpoint
        else Policy Failed
            Policy-->>MW: Unauthorized
            MW-->>BFS: HTTP 401 Unauthorized
        end
    else Token Invalid
        MW-->>BFS: HTTP 401 Unauthorized
    end
```

## Hosting and Dependency Injection

This diagram shows how the library integrates with ASP.NET Core's hosting and dependency injection:

```mermaid
graph TB
    subgraph "Application Startup"
        WB[WebApplicationBuilder]
        WA[WebApplication]
    end

    subgraph "Service Registration"
        MSE["MessageLoopServiceCollectionExtensions<br/>.AddMessageLoop<T>"]
        WAS["WebApiSecurity<br/>.AddBotFrameworkAuthentication"]
    end

    subgraph "Dependency Injection Container"
        DI[IServiceCollection]
        TBA_S[TeamsBotApplication<br/>Singleton]
        CC_S[ConversationClient<br/>Scoped]
        Auth_S[Authentication Services]
        HTTP_S[IHttpClientFactory]
        Token_S[IAuthorizationHeaderProvider]
    end

    subgraph "Application Configuration"
        ABE["AppBuilderExtensions<br/>.UseBotApplication<T>"]
        EP["/api/messages Endpoint"]
    end

    subgraph "Runtime"
        TBA_I[TeamsBotApplication Instance]
        CC_I[ConversationClient Instance]
        HTTP_I[HttpClient]
    end

    WB -->|builder.Services| DI
    WB -->|.AddMessageLoop| MSE
    WB -->|.AddBotFrameworkAuthentication| WAS
    
    MSE -->|Register| TBA_S
    MSE -->|Register| CC_S
    WAS -->|Register| Auth_S
    
    DI -->|Provides| HTTP_S
    DI -->|Provides| Token_S
    
    WB -->|.Build| WA
    WA -->|.UseBotApplication| ABE
    
    ABE -->|Resolve from DI| TBA_I
    ABE -->|Configure| EP
    ABE -->|Add Middleware| Auth_S
    
    EP -->|Uses| TBA_I
    TBA_I -->|Requests| CC_I
    CC_I -->|Creates| HTTP_I
    CC_I -->|Uses| Token_S
    
    style WB fill:#e1f5ff
    style WA fill:#e1f5ff
    style MSE fill:#e1ffe1
    style WAS fill:#e1ffe1
    style TBA_S fill:#fff4e1
    style CC_S fill:#ffe1f5
```

## Activity Processing Pipeline

This flowchart shows the complete activity processing logic within BotApplication:

```mermaid
flowchart TD
    Start([Activity Received]) --> Parse[Parse Activity<br/>from HTTP Request]
    Parse --> Store[Store in<br/>LastActivity]
    Store --> CheckType{Activity.Type?}
    
    CheckType -->|message| RaiseEvent1[Raise OnNewActivity Event]
    CheckType -->|messageReaction| RaiseEvent2[Raise OnNewActivity Event]
    CheckType -->|installationUpdate<br/>Teams Only| RaiseEvent3[Raise OnNewActivity Event]
    CheckType -->|other| LogWarning[Log Unsupported Type]
    
    RaiseEvent1 --> CheckHandler1{OnMessage<br/>Handler Set?}
    CheckHandler1 -->|Yes| InvokeMsg[Invoke OnMessage<br/>with Activity]
    CheckHandler1 -->|No| End
    
    RaiseEvent2 --> CreateWrapper1[Create<br/>MessageReactionActivityWrapper]
    CreateWrapper1 --> CheckHandler2{OnMessageReaction<br/>Handler Set?}
    CheckHandler2 -->|Yes| InvokeReaction[Invoke OnMessageReaction<br/>with Wrapper]
    CheckHandler2 -->|No| End
    
    RaiseEvent3 --> ConvertTeams[Convert to<br/>TeamsActivity]
    ConvertTeams --> CreateWrapper2[Create<br/>InstallationUpdateWrapper]
    CreateWrapper2 --> CheckHandler3{OnInstallationUpdate<br/>Handler Set?}
    CheckHandler3 -->|Yes| InvokeInstall[Invoke OnInstallationUpdate<br/>with Wrapper]
    CheckHandler3 -->|No| End
    
    InvokeMsg --> HandlerLogic[Application Handler Logic]
    InvokeReaction --> HandlerLogic
    InvokeInstall --> HandlerLogic
    
    HandlerLogic --> CheckSend{Send Reply?}
    CheckSend -->|Yes| SendActivity[SendActivityAsync]
    CheckSend -->|No| End
    
    SendActivity --> GetToken[Get Bot Framework Token]
    GetToken --> PrepareRequest[Prepare HTTP POST Request]
    PrepareRequest --> CallAPI["POST /v3/conversations/<br/>{id}/activities"]
    CallAPI --> End([Complete])
    
    LogWarning --> End
    
    style Start fill:#e1f5ff
    style End fill:#e1f5ff
    style HandlerLogic fill:#fff4e1
    style SendActivity fill:#ffe1f5
    style CallAPI fill:#f5e1ff
```

## Schema Extensibility Pattern

This diagram illustrates how the library uses generic types and extension data to support channel-specific data:

```mermaid
graph TB
    subgraph "Generic Activity Model"
        AT["Activity<T> where T: ChannelData"]
        ACD[ChannelData]
        EP[ExtensionData Properties]
    end

    subgraph "Core Implementations"
        A["Activity<br/>Activity<ChannelData>"]
    end

    subgraph "Teams Implementations"
        TA["TeamsActivity<br/>Activity<TeamsChannelData>"]
        TCD["TeamsChannelData<br/>: ChannelData"]
    end

    subgraph "Extension Data Pattern"
        ED1[JsonExtensionData]
        ED2[Properties Dictionary]
        ED3[Runtime Deserialization]
    end

    AT -->|Type Parameter| ACD
    AT -->|Contains| EP
    
    A -->|Concrete Type| AT
    A -->|Uses| ACD
    
    TA -->|Concrete Type| AT
    TA -->|Uses| TCD
    TCD -->|Inherits| ACD
    
    ACD -->|Decorated with| ED1
    ACD -->|Stores Unknown Fields| ED2
    
    TA -->|Parses from| A
    TA -->|Converts| ED2
    TA -->|Creates| TCD
    TCD -->|Extracts| ED3
    
    style AT fill:#e1ffe1
    style ACD fill:#e1ffe1
    style A fill:#ffe1f5
    style TA fill:#fff4e1
    style TCD fill:#fff4e1
    style ED1 fill:#f5e1ff
```

## Key Design Patterns

### 1. **Event-Driven Architecture**
The library uses C# events and action delegates to allow applications to respond to bot activities without subclassing:
- `OnNewActivity` - Event raised for all activities
- `OnMessage` - Action for message activities
- `OnMessageReaction` - Action for reaction activities
- `OnInstallationUpdate` - Action for Teams installation updates (Teams extension)

### 2. **Wrapper Pattern**
Complex activities are wrapped in specialized classes that extract and expose relevant data:
- `MessageReactionActivityWrapper` - Extracts reaction data from activity properties
- `InstallationUpdateWrapper` - Extracts Teams installation data and provides helper properties

### 3. **Extension Data Pattern**
JSON extension data is used throughout to preserve channel-specific fields:
- Unknown JSON properties are captured in `Properties` dictionaries
- Teams extension converts generic activities to strongly-typed models
- Allows forward compatibility with Bot Framework protocol changes

### 4. **Generic Type Constraints**
The `Activity<T>` generic class constrains channel data types:
- Base: `Activity<ChannelData>`
- Teams: `Activity<TeamsChannelData>`
- Enables type-safe channel-specific extensions

### 5. **Dependency Injection Integration**
Full integration with ASP.NET Core DI:
- Extension methods for service registration
- Scoped and singleton lifetime management
- Factory patterns for HTTP clients

### 6. **Minimal API Surface**
The library exposes a minimal, focused API:
- Single endpoint (`/api/messages`)
- Simple registration (`AddMessageLoop<T>()`)
- Simple configuration (`UseBotApplication<T>()`)
- Action-based handlers (no complex interfaces)

## Technology Stack

### Core Dependencies
- **.NET 9.0** - Latest .NET runtime
- **ASP.NET Core** - Web framework and hosting
- **System.Text.Json** - JSON serialization

### Authentication & Identity
- **Microsoft.Identity.Web** - Microsoft identity platform integration
- **Microsoft.AspNetCore.Authentication.JwtBearer** - JWT token validation
- **Microsoft.AspNetCore.Authentication.OpenIdConnect** - OpenID Connect support
- **Microsoft.Identity.Web.DownstreamApi** - Bot Framework API calls

### Bot Framework
- **Bot Framework Service API** - Cloud service for bot communication
- **Bot Framework Protocol** - Activity schema and conversation protocol

## Deployment Architecture

```mermaid
graph TB
    subgraph "Azure Cloud"
        subgraph "Bot Service"
            BFS[Azure Bot Service]
            BSR[Bot Service Registration]
        end
        
        subgraph "Identity"
            AAD[Microsoft Entra ID<br/>Azure AD]
            APP[App Registration]
        end
        
        subgraph "Hosting"
            ASVC[Azure App Service<br/>or Container]
            BOT[Bot Application]
        end
    end
    
    subgraph "Channels"
        Teams[Microsoft Teams]
        WebChat[Web Chat]
        Other[Other Channels]
    end
    
    subgraph "Development"
        Dev[Local Development]
        Tunnel[Tunneling Service<br/>ngrok/devtunnel]
    end
    
    Teams -->|User Messages| BFS
    WebChat -->|User Messages| BFS
    Other -->|User Messages| BFS
    
    BFS -->|Forward Activity| ASVC
    ASVC -->|JWT Bearer| AAD
    AAD -->|Validate| APP
    ASVC -->|Runs| BOT
    
    BOT -->|Get Token| AAD
    BOT -->|Send Activity| BFS
    
    BSR -.->|Configuration| BFS
    APP -.->|Client ID/Secret| BOT
    
    Dev -->|Expose via| Tunnel
    Tunnel -.->|Public URL| BFS
    
    style BFS fill:#e1f5ff
    style AAD fill:#ffe1f5
    style BOT fill:#fff4e1
    style Teams fill:#f5e1ff
```

## Summary

Rido.BFLite provides a clean, minimal architecture for building Bot Framework bots:

1. **Core Package** - Essential bot functionality, activity schema, and Bot Framework integration
2. **Teams Extension** - Teams-specific features building on the core
3. **Hosting Integration** - Seamless ASP.NET Core integration with authentication
4. **Event-Driven Model** - Simple action-based handlers for bot logic
5. **Type Safety** - Strong typing for activities and channel data with extensibility

The architecture emphasizes:
- **Simplicity** - Minimal concepts and API surface
- **Type Safety** - Leverages C# type system
- **Extensibility** - Generic types and extension data for channel-specific features
- **Modern .NET** - Latest .NET features and patterns
- **Security** - Built-in Bot Framework authentication and authorization
