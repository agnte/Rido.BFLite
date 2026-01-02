# Rido.BFLite Go Implementation

A lightweight Go library for building Microsoft Bot Framework bots with minimal overhead.

This is a Go port of the [Rido.BFLite](../) library, following the [TRANSLATION_SPEC.md](../specs/TRANSLATION_SPEC.md).

## Installation

```bash
go get github.com/agnte/Rido.BFLite/go/bflite
```

## Quick Start

```go
package main

import (
    "context"
    "fmt"
    "log"

    "github.com/agnte/Rido.BFLite/go/bflite/teams"
    "github.com/agnte/Rido.BFLite/go/bflite/teams/handlers"
)

func main() {
    // Create bot with configuration
    bot := teams.NewTeamsBotApplication(teams.Config{
        ClientID:     "your-app-id",
        ClientSecret: "your-app-secret",
    })

    // Set up message handler
    bot.OnMessage = func(ctx context.Context, c *handlers.Context) error {
        _, err := c.SendActivity(ctx, fmt.Sprintf("You said: %s", c.Activity.Text))
        return err
    }

    // Run the application
    log.Fatal(bot.Listen(":3978"))
}
```

## Features

- ✅ Core Bot Framework Activity handling
- ✅ Teams-specific activity types and channel data
- ✅ Message handlers
- ✅ Message reaction handlers
- ✅ Installation update handlers
- ✅ Conversation update handlers
- ✅ Middleware pipeline support
- ✅ OAuth 2.0 client credentials authentication
- ✅ Extension data preservation for unknown JSON properties

## Architecture

The library follows a similar structure to the C# implementation:

```
bflite/
├── schema/                   # Core Bot Framework models
│   ├── activity.go           # Activity model with JSON serialization
│   ├── conversation.go       # Conversation context
│   ├── conversation_account.go # User/bot account info
│   └── channel_data.go       # Base channel data
├── teams/                    # Microsoft Teams extensions
│   ├── schema/               # Teams-specific models
│   │   ├── teams_activity.go
│   │   ├── teams_channel_data.go
│   │   ├── teams_conversation.go
│   │   └── teams_conversation_account.go
│   ├── handlers/             # Handler types
│   │   └── handlers.go
│   ├── teams_bot_application.go
│   └── auth.go               # OAuth token management
├── bot_application.go        # Main bot class
└── conversation_client.go    # HTTP client for sending activities
```

## Configuration

Configuration can be provided via the `Config` struct or environment variables:

| Setting | Environment Variable | Description |
|---------|---------------------|-------------|
| ClientID | `CLIENT_ID` | Azure AD application ID |
| ClientSecret | `CLIENT_SECRET` | Azure AD client secret |
| TenantID | `TENANT_ID` | Azure AD tenant (optional) |

## Handlers

### Message Handler

```go
bot.OnMessage = func(ctx context.Context, c *handlers.Context) error {
    text := c.Activity.Text
    _, err := c.SendActivity(ctx, fmt.Sprintf("Echo: %s", text))
    return err
}
```

### Message Reaction Handler

```go
bot.OnMessageReaction = func(ctx context.Context, args *handlers.MessageReactionArgs, c *handlers.Context) error {
    for _, reaction := range args.ReactionsAdded {
        log.Printf("Reaction added: %s", reaction.Type)
    }
    return nil
}
```

### Installation Update Handler

```go
bot.OnInstallationUpdate = func(ctx context.Context, args *handlers.InstallationUpdateArgs, c *handlers.Context) error {
    if args.IsAdd() {
        _, err := c.SendActivity(ctx, "Thanks for installing!")
        return err
    }
    return nil
}
```

### Conversation Update Handler

```go
bot.OnConversationUpdate = func(ctx context.Context, args *handlers.ConversationUpdateArgs, c *handlers.Context) error {
    for _, member := range args.MembersAdded {
        _, err := c.SendActivity(ctx, fmt.Sprintf("Welcome, %s!", member.Name))
        if err != nil {
            return err
        }
    }
    return nil
}
```

## Middleware

Implement the `ITurnMiddleware` interface to create custom middleware:

```go
type LoggingMiddleware struct{}

func (m *LoggingMiddleware) OnTurn(ctx context.Context, bot *bflite.BotApplication, activity *schema.Activity, next bflite.NextDelegate) error {
    log.Printf("Received activity: %s", activity.Type)
    
    err := next(ctx) // Continue to next middleware/handler
    
    log.Printf("Finished processing activity: %s", activity.Type)
    return err
}

// Register middleware
bot.Use(&LoggingMiddleware{})
```

## Examples

See the [examples](examples/) directory for complete examples:

- [Echo Bot](examples/echobot/) - Simple echo bot demonstrating all handler types

## Running the Echo Bot Example

```bash
cd examples/echobot
export CLIENT_ID="your-app-id"
export CLIENT_SECRET="your-app-secret"
go run main.go
```

## Testing

Run tests with:

```bash
cd bflite
go test ./... -v
```

## License

See the main repository license.
