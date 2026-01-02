# Rido.BFLite Translation Specification

**Purpose**: Enable LLM-driven translation of Rido.BFLite to other programming languages  
**Created**: 2026-01-02  
**Status**: Complete  
**Source Language**: C# / .NET 9.0

## Overview

Rido.BFLite is a lightweight library for building Microsoft Bot Framework bots with minimal overhead. This specification documents the library's design, APIs, and behaviors to enable accurate translation to other programming languages like Python, TypeScript/Node.js, Go, Java, or Rust.

---

## Core Concepts

### 1. Bot Framework Protocol

The library implements the Microsoft Bot Framework REST API protocol:

- **Activities**: Messages and events exchanged between bot and Bot Framework Service
- **Conversations**: Container for message exchanges between users and bots
- **Channels**: Communication platforms (Teams, Web Chat, Slack, etc.)
- **Service URL**: Per-conversation endpoint for sending activities back to the channel

### 2. Activity Types

| Type | Description |
|------|-------------|
| `message` | Text message from user or bot |
| `messageReaction` | Emoji reaction added/removed from a message |
| `conversationUpdate` | Members added/removed from conversation |
| `installationUpdate` | Bot installed/uninstalled (Teams-specific) |
| `invoke` | Special request requiring synchronous response |
| `trace` | Debug/diagnostic activity (not sent to users) |

---

## Package Structure

```text
Rido.BFLite/
├── Rido.BFLite.Core/           # Core bot functionality
│   ├── BotApplication.cs       # Main bot class
│   ├── ConversationClient.cs   # HTTP client for sending activities
│   ├── UserTokenClient.cs      # OAuth token management
│   ├── Schema/                 # Activity models
│   │   ├── Activity.cs
│   │   ├── ChannelData.cs
│   │   ├── Conversation.cs
│   │   └── ConversationAccount.cs
│   └── Hosting/                # ASP.NET Core integration
│       ├── AppBuilderExtensions.cs
│       ├── BotApplicationConfigurationExtensions.cs
│       ├── BotAuthenticationHandler.cs
│       └── JwtExtensions.cs
├── Rido.BFLite.Teams/          # Microsoft Teams extensions
│   ├── TeamsBotApplication.cs  # Teams-specific bot class
│   ├── Context.cs              # Request context wrapper
│   ├── Schema/                 # Teams-specific models
│   │   ├── TeamsActivity.cs
│   │   ├── TeamsChannelData.cs
│   │   └── ...
│   └── Handlers/               # Handler delegate types
│       ├── MessageHandler.cs
│       ├── MessageReactionHandler.cs
│       ├── InstallationUpdateHandler.cs
│       └── ConversationUpdateHandler.cs
└── Rido.BFLite.Compat/         # Bot Framework SDK compatibility layer
```

---

## User Scenarios & Testing

### User Story 1 - Echo Bot (Priority: P1) 🎯 MVP

A developer creates a simple echo bot that responds to user messages.

**Why this priority**: This is the most basic bot functionality and validates the core library works.

**Independent Test**: Bot receives "hello" message and responds "you said hello"

**Acceptance Scenarios**:

1. **Given** a running bot application, **When** a user sends "hello", **Then** the bot responds with "you said hello"
2. **Given** a running bot application, **When** a user sends an empty message, **Then** the bot handles it gracefully (no crash)
3. **Given** a running bot application, **When** Bot Framework sends an activity, **Then** JWT authentication validates the token

---

### User Story 2 - Proactive Messaging (Priority: P2)

A developer sends a proactive message to a user outside of a direct conversation turn.

**Why this priority**: Proactive messaging is essential for notifications and bot-initiated conversations.

**Independent Test**: Bot can send a message using stored conversation reference

**Acceptance Scenarios**:

1. **Given** a stored conversation reference, **When** the bot sends a proactive message, **Then** the user receives the message
2. **Given** an invalid conversation reference, **When** the bot sends a message, **Then** an error is returned

---

### User Story 3 - Handle Message Reactions (Priority: P3)

The bot responds when a user adds or removes a reaction to a message.

**Why this priority**: Reactions enable richer interaction patterns.

**Acceptance Scenarios**:

1. **Given** a message in conversation, **When** user adds a thumbs-up reaction, **Then** `OnMessageReaction` handler is invoked with reaction details
2. **Given** a message in conversation, **When** user removes a reaction, **Then** handler receives `ReactionsRemoved` list

---

### User Story 4 - Conversation Updates (Priority: P3)

The bot is notified when members join or leave a conversation.

**Acceptance Scenarios**:

1. **Given** a group conversation, **When** a new member joins, **Then** `OnConversationUpdate` handler receives `MembersAdded`
2. **Given** a group conversation, **When** a member leaves, **Then** handler receives `MembersRemoved`

---

### User Story 5 - Teams Installation Events (Priority: P4)

The bot handles Teams-specific installation/uninstallation events.

**Acceptance Scenarios**:

1. **Given** bot is installed in Teams, **When** installation completes, **Then** `OnInstallationUpdate` handler is invoked with `Action = "add"`
2. **Given** bot is uninstalled, **When** uninstallation completes, **Then** handler receives `Action = "remove"`

---

### User Story 6 - Middleware Pipeline (Priority: P2)

A developer adds custom middleware to process activities before/after handlers.

**Acceptance Scenarios**:

1. **Given** middleware is registered, **When** activity is received, **Then** middleware executes before handler
2. **Given** multiple middleware, **When** activity is received, **Then** middleware executes in registration order
3. **Given** middleware calls `next()`, **When** processing continues, **Then** subsequent middleware and handler execute

---

## Functional Requirements

### FR-001: Activity Serialization

System MUST serialize/deserialize activities using JSON with:

- Property naming: camelCase
- Null handling: Ignore null values when writing
- Extension data: Preserve unknown properties in a dictionary

### FR-002: HTTP Endpoint

System MUST expose a POST endpoint (default: `/api/messages`) that:

- Accepts Activity JSON in request body
- Returns activity ID on success
- Requires JWT authentication

### FR-003: JWT Authentication

System MUST validate incoming requests using:

- Issuer: `api.botframework.com` or `https://sts.windows.net/{tenantId}/`
- Audience: Bot's Client ID (App ID)
- OpenID configuration from: `https://login.botframework.com/v1/.well-known/openid-configuration`

### FR-004: Outbound Authentication

System MUST authenticate outbound requests using:

- OAuth 2.0 client credentials flow
- Scope: `https://api.botframework.com/.default`
- Token endpoint: Microsoft identity platform

### FR-005: Activity Reply

System MUST provide a method to create reply activities that:

- Copies `Conversation`, `ServiceUrl`, `ChannelId` from original
- Swaps `From` and `Recipient`
- Sets `ReplyToId` to original activity ID

### FR-006: Send Activity

System MUST send activities via HTTP POST to:

- URL: `{ServiceUrl}v3/conversations/{ConversationId}/activities/`
- Content-Type: `application/json`
- Authorization: Bearer token

### FR-007: Handler Invocation

System MUST invoke appropriate handlers based on activity type:

- `message` → `OnMessage`
- `messageReaction` → `OnMessageReaction`
- `conversationUpdate` → `OnConversationUpdate`
- `installationUpdate` → `OnInstallationUpdate` (Teams only)

### FR-008: Middleware Pipeline

System MUST support middleware that:

- Executes in registration order
- Receives `BotApplication`, `Activity`, and `NextDelegate`
- Can short-circuit by not calling `next()`
- Can modify activity before/after handler

### FR-009: Teams Channel Data

For Teams channel, System MUST parse additional data:

- `TeamsChannelData.Settings.SelectedChannel.Id`
- `TeamsChannelData.Tenant.Id`
- `TeamsChannelData.Team.Id`
- `TeamsConversationAccount.UserPrincipalName`

### FR-010: Error Handling

System MUST wrap handler exceptions in `BotHandlerException` with:

- Original exception as inner exception
- Reference to the activity that caused the error

---

## Key Entities

### Activity

The core message/event model exchanged with Bot Framework.

```text
Activity
├── type: string              # "message", "messageReaction", etc.
├── id: string?               # Unique identifier
├── serviceUrl: string?       # Callback URL for this conversation
├── channelId: string?        # "msteams", "webchat", "slack", etc.
├── text: string?             # Message text content
├── replyToId: string?        # ID of message being replied to
├── from: ConversationAccount?    # Sender information
├── recipient: ConversationAccount?  # Recipient information
├── conversation: Conversation?   # Conversation context
├── channelData: ChannelData?     # Channel-specific data
├── entities: JsonArray?          # Mentions, card actions, etc.
└── [extensionData]: Dictionary   # Unknown properties preserved
```

### ConversationAccount

Represents a user or bot in a conversation.

```text
ConversationAccount
├── id: string?               # Unique identifier
├── name: string?             # Display name
├── aadObjectId: string?      # Azure AD object ID
├── role: string?             # "user" or "bot"
└── [extensionData]: Dictionary
```

### Conversation

Represents the conversation context.

```text
Conversation
├── id: string?               # Conversation identifier
└── [extensionData]: Dictionary
```

### ChannelData

Base class for channel-specific data (extended by TeamsChannelData).

```text
ChannelData
├── clientActivityId: string?
└── [extensionData]: Dictionary
```

### TeamsChannelData (extends ChannelData)

Teams-specific conversation metadata.

```text
TeamsChannelData
├── tenant: TeamsChannelDataTenant?
│   └── id: string?
├── team: Team?
│   ├── id: string?
│   └── name: string?
├── channel: TeamsChannel?
│   ├── id: string?
│   └── name: string?
├── settings: TeamsChannelDataSettings?
│   └── selectedChannel: TeamsChannel?
└── [inherited from ChannelData]
```

### TeamsConversationAccount (extends ConversationAccount)

Teams user with additional properties.

```text
TeamsConversationAccount
├── userPrincipalName: string?  # UPN (email)
├── email: string?
└── [inherited from ConversationAccount]
```

---

## API Surface

### BotApplication Class

```text
class BotApplication:
    # Properties
    UserTokenClient: UserTokenClient          # OAuth token management
    OnActivity: Func<Activity, CancellationToken, Task>?  # Low-level activity callback
        # Note: Typically set internally by TeamsBotApplication
        # Users normally interact with typed handlers (OnMessage, etc.)

    # Methods
    ProcessAsync(httpContext, cancellationToken) -> Activity
        # Parse activity from request body
        # Run middleware pipeline
        # Invoke OnActivity callback
        # Return processed activity

    SendActivityAsync(activity, cancellationToken) -> string
        # Send activity to Bot Framework
        # Return activity ID from response

    Use(middleware) -> ITurnMiddleware
        # Register middleware component
        # Return middleware for chaining
```

### TeamsBotApplication Class (extends BotApplication)

```text
class TeamsBotApplication extends BotApplication:
    # Handler Properties
    OnMessage: MessageHandler?              # Message received
    OnMessageReaction: MessageReactionHandler?  # Reaction added/removed
    OnInstallationUpdate: InstallationUpdateHandler?  # Bot installed/removed
    OnConversationUpdate: ConversationUpdateHandler?  # Members changed

    # Constructor
    constructor(config, logger, serviceKey="AzureAd"):
        # Set up OnActivity to dispatch to appropriate handlers
        # based on activity.Type
```

### Context Class

```text
class Context:
    # Properties
    BotApplication: TeamsBotApplication     # Reference to bot
    Activity: TeamsActivity                 # Current activity

    # Methods
    SendActivityAsync(text, cancellationToken) -> string
        # Create reply activity with text
        # Send via BotApplication
```

### Handler Delegate Types

```text
delegate MessageHandler(context: Context, cancellationToken) -> Task
delegate MessageReactionHandler(args: MessageReactionArgs, context: Context, cancellationToken) -> Task
delegate InstallationUpdateHandler(args: InstallationUpdateArgs, context: Context, cancellationToken) -> Task
delegate ConversationUpdateHandler(args: ConversationUpdateArgs, context: Context, cancellationToken) -> Task
```

### ITurnMiddleware Interface

```text
interface ITurnMiddleware:
    OnTurnAsync(botApplication, activity, next, cancellationToken) -> Task
```

---

## Hosting Integration

### Service Registration

```text
# Register bot services
services.AddBotApplication<TeamsBotApplication>()
    # Registers:
    # - TeamsBotApplication as singleton
    # - ConversationClient as keyed scoped service
    # - UserTokenClient as scoped
    # - HttpClient with authentication handler
    # - JWT authentication with Bot Framework validation
```

### Application Configuration

```text
# Configure bot endpoint
app.UseBotApplication<TeamsBotApplication>(
    routePath: "api/messages",        # HTTP endpoint path
    authorizationPolicy: "DefaultPolicy"  # Authorization policy name
)
    # Sets up:
    # - Authentication middleware
    # - Authorization middleware
    # - POST endpoint with JWT protection
```

### Configuration Settings

```json
{
  "AzureAd": {
    "ClientId": "bot-app-id",
    "ClientSecret": "bot-app-secret",
    "TenantId": "tenant-id-or-common"
  },
  "ASPNETCORE_URLS": "https://localhost:5001"
}
```

---

## Success Criteria

### SC-001: Functional Parity

Translated implementation MUST pass all acceptance scenarios defined in user stories.

### SC-002: Protocol Compliance

Translated implementation MUST correctly serialize/deserialize all activity types per Bot Framework protocol.

### SC-003: Authentication

Translated implementation MUST validate incoming JWT tokens and authenticate outbound requests.

### SC-004: Idiomatic Code

Translated implementation SHOULD follow target language idioms and conventions (e.g., Python naming, Go interfaces, TypeScript types).

### SC-005: Minimal Dependencies

Translated implementation SHOULD minimize external dependencies while maintaining functionality.

### SC-006: Example Compatibility

A translated echo bot example MUST produce identical behavior to the C# sample when deployed.

---

## Implementation Notes for Translation

### JSON Serialization

- Use language-native JSON libraries (Python `json`, Go `encoding/json`, TypeScript native JSON)
- Configure camelCase property naming
- Preserve unknown properties via extension data mechanism
- Handle nullable fields appropriately

### HTTP Client

- Use language-native HTTP libraries
- Implement retry logic for transient failures
- Handle token refresh for long-running operations

### Async/Await Patterns

- C# uses `async/await` with `Task<T>`
- Python: Use `asyncio` with `async/await`
- Go: Use goroutines and channels, or context
- TypeScript: Use `Promise<T>` with `async/await`
- Java: Use `CompletableFuture<T>` or reactive patterns

### Dependency Injection

- C# uses Microsoft.Extensions.DependencyInjection
- Python: Use `dependency-injector` or similar
- Go: Use constructor injection or wire
- TypeScript: Use `tsyringe` or similar
- Adapt patterns to target language conventions

### JWT Validation

- Use established JWT libraries in target language
- Fetch OpenID configuration from Bot Framework
- Validate issuer, audience, signature, and expiration

### Middleware Pattern

- Implement chain-of-responsibility pattern
- Support async execution
- Allow short-circuiting via `next()` call control

---

## Sample: Echo Bot (Reference Implementation)

```csharp
using Rido.BFLite.Core.Hosting;
using Rido.BFLite.Teams;

// 1. Create web application builder
var builder = WebApplication.CreateSlimBuilder(args);

// 2. Register bot services (includes auth setup)
builder.Services.AddBotApplication<TeamsBotApplication>();

// 3. Build the application
var app = builder.Build();

// 4. Configure bot endpoint and get bot instance
var bot = app.UseBotApplication<TeamsBotApplication>();

// 5. Set up message handler
bot.OnMessage = async (context, cancellationToken) =>
{
    // Create and send reply
    await context.SendActivityAsync(
        $"You said: {context.Activity.Text}",
        cancellationToken
    );
};

// 6. Run the application
app.Run();
```

### Equivalent Python (Target)

```python
from bflite import TeamsBotApplication, create_app

# Create and configure app
app = create_app()
bot = TeamsBotApplication(app)

# Set up message handler
@bot.on_message
async def handle_message(context):
    await context.send_activity(f"You said: {context.activity.text}")

# Run the application
if __name__ == "__main__":
    app.run()
```

### Equivalent TypeScript (Target)

```typescript
import { TeamsBotApplication, createApp } from 'bflite';

// Create and configure app
const app = createApp();
const bot = new TeamsBotApplication(app);

// Set up message handler
bot.onMessage = async (context) => {
    await context.sendActivity(`You said: ${context.activity.text}`);
};

// Run the application
app.listen(3978);
```

---

## Appendix: Bot Framework REST API Reference

### Receive Activity

```http
POST /api/messages
Authorization: Bearer {jwt-token}
Content-Type: application/json

{
  "type": "message",
  "id": "activity-id",
  "serviceUrl": "https://smba.trafficmanager.net/...",
  "channelId": "msteams",
  "from": { "id": "user-id", "name": "User Name" },
  "recipient": { "id": "bot-id", "name": "Bot Name" },
  "conversation": { "id": "conversation-id" },
  "text": "Hello bot!"
}
```

### Send Activity

```http
POST {serviceUrl}v3/conversations/{conversationId}/activities/
Authorization: Bearer {oauth-token}
Content-Type: application/json

{
  "type": "message",
  "text": "Hello user!",
  "from": { "id": "bot-id", "name": "Bot Name" },
  "recipient": { "id": "user-id", "name": "User Name" },
  "conversation": { "id": "conversation-id" },
  "replyToId": "original-activity-id"
}
```

### Response

```json
{
  "id": "new-activity-id"
}
```

---

## Appendix: Authentication Endpoints

### OpenID Configuration

```text
https://login.botframework.com/v1/.well-known/openid-configuration
```

### Token Endpoint (for outbound)

```text
https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token

POST body:
  grant_type=client_credentials
  client_id={botAppId}
  client_secret={botAppSecret}
  scope=https://api.botframework.com/.default
```

---

## Appendix: Teams-Specific Activity Properties

### Installation Update

```json
{
  "type": "installationUpdate",
  "action": "add",
  "channelData": {
    "settings": {
      "selectedChannel": {
        "id": "channel-id"
      }
    },
    "tenant": {
      "id": "tenant-id"
    }
  }
}
```

### Message Reaction

```json
{
  "type": "messageReaction",
  "reactionsAdded": [
    { "type": "like" }
  ],
  "reactionsRemoved": []
}
```

### Conversation Update

```json
{
  "type": "conversationUpdate",
  "membersAdded": [
    { "id": "user-id", "name": "User Name" }
  ],
  "membersRemoved": []
}
```
