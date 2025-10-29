# Rido.BFLite

A lightweight library for building Microsoft Bot Framework bots with minimal overhead and maximum simplicity.

## Overview

Rido.BFLite provides a streamlined API for creating Bot Framework bots without the complexity of the full Bot Framework SDK. It's designed for developers who want to build bots quickly with a minimal, focused API surface.

## Features

- 🚀 **Lightweight**: Minimal dependencies and overhead
- 🔒 **Secure**: Built-in Bot Framework authentication and JWT validation
- 📦 **Modular**: Core library with optional Teams extensions
- ⚡ **Modern**: Built on .NET 9.0 with ASP.NET Core
- 🎯 **Simple API**: Event-driven programming model

## Packages

- **Rido.BFLite.Core**: Core bot functionality and Bot Framework integration
- **Rido.BFLite.Teams**: Microsoft Teams-specific features and extensions

## Getting Started

### Installation

```bash
# Core functionality
dotnet add package Rido.BFLite.Core

# Teams support
dotnet add package Rido.BFLite.Teams
```

### Quick Start

Create a simple echo bot in just a few lines:

```csharp
using Rido.BFLite.Core;
using Rido.BFLite.Core.Hosting;
using Rido.BFLite.Core.Schema;
using Rido.BFLite.Teams;

WebApplicationBuilder webAppBuilder = WebApplication.CreateSlimBuilder(args);
webAppBuilder.Services.AddBotFrameworkAuthentication();
webAppBuilder.Services.AddMessageLoop<TeamsBotApplication>();
WebApplication webApp = webAppBuilder.Build();
TeamsBotApplication botApp = webApp.UseBotApplication<TeamsBotApplication>();

botApp.OnMessage = async activity =>
{
    Activity reply = activity.CreateReplyActivity($"you said {activity.Text}, with ❤️ at {DateTime.Now:T}");
    await botApp.SendActivityAsync(reply);
};

webApp.Run();
```

## Configuration

Configure your bot by adding the following settings to `appsettings.json`:

```json
{
  "AzureAd": {
    "ClientId": "your-bot-app-id",
    "ClientSecret": "your-bot-app-secret"
  },
  "ASPNETCORE_URLS": "https://localhost:5001"
}
```

## Usage Examples

### Handle Message Reactions

```csharp
botApp.OnMessageReaction = async reaction =>
{
    string result = $"Reaction received at {DateTime.Now:T}. " +
                   $"Added: {reaction.ReactionsAdded?.FirstOrDefault()?.Type} " +
                   $"Removed: {reaction.ReactionsRemoved?.FirstOrDefault()?.Type}";

    Activity reply = reaction.Activity.CreateReplyActivity(result);
    await botApp.SendActivityAsync(reply);
};
```

### Handle Conversation Updates

```csharp
botApp.OnConversationUpdate = conversationUpdate =>
{
    if (conversationUpdate.MembersAdded != null)
    {
        foreach (var member in conversationUpdate.MembersAdded)
        {
            Console.WriteLine($"Member added: {member.Id} - {member.Name}");
        }
    }
    if (conversationUpdate.MembersRemoved != null)
    {
        foreach (var member in conversationUpdate.MembersRemoved)
        {
            Console.WriteLine($"Member removed: {member.Id} - {member.Name}");
        }
    }
};
```

### Handle Teams Installation Updates

```csharp
botApp.OnInstallationUpdate = installationUpdate =>
{
    Console.WriteLine($"Installation update event. Action: {installationUpdate.Action} for {installationUpdate.SelectedChannelId} channel");
};
```

### Handle All Activities

```csharp
botApp.OnNewActivity += (sender, args) =>
{
    Console.WriteLine($"New activity received of type {args.Activity.Type} from {args.Activity.From?.Id}");
};
```

### Advanced ConversationClient Usage

For advanced scenarios, inject `ConversationClient` directly:

```csharp
// Update a previously sent message
ResourceResponse originalResponse = await botApp.SendActivityAsync(message);
message.Id = originalResponse.Id;
message.Text = "Updated message text";
await conversationClient.UpdateActivityAsync(message);

// Delete a message
await conversationClient.DeleteActivityAsync(
    serviceUrl: activity.ServiceUrl,
    conversationId: activity.Conversation.Id,
    activityId: activityIdToDelete);

// Create a proactive conversation
var parameters = new ConversationParameters
{
    Bot = new ConversationAccount { Id = botId, Name = "My Bot" },
    Members = new List<ConversationAccount>
    {
        new ConversationAccount { Id = userId, Name = "User" }
    },
    TopicName = "Proactive Message"
};
ConversationResourceResponse conversation = await conversationClient.CreateConversationAsync(
    serviceUrl, parameters);

// Get conversation members with pagination
PagedMembersResult members = await conversationClient.GetConversationPagedMembersAsync(
    serviceUrl, conversationId, pageSize: 50);
```

## Architecture

The library consists of:

- **BotApplication**: Base class for bot applications
- **TeamsBotApplication**: Extended bot application with Teams-specific features
- **ConversationClient**: Complete implementation of Bot Framework REST Connector API v3, supporting all conversation operations with JWT authentication
- **Activity Schema**: Strongly-typed models for Bot Framework activities
- **Hosting Extensions**: ASP.NET Core integration and authentication

### ConversationClient Features

The `ConversationClient` provides comprehensive Bot Framework REST API v3 support:

- **Send Activities**: Send messages to conversations
- **Reply to Activities**: Send threaded replies to specific messages
- **Update Activities**: Edit previously sent messages
- **Delete Activities**: Remove messages from conversations
- **Create Conversations**: Start new conversations for proactive messaging
- **Get Members**: Retrieve conversation and activity participants
- **Paged Members**: Efficiently handle large group memberships with pagination
- **Upload Attachments**: Send files and rich media

All operations use secure JWT token authentication via Microsoft Identity Platform.

For detailed architecture diagrams and explanations, see the [Architecture Documentation](docs/ARCHITECTURE.md).

## Requirements

- .NET 9.0 or later
- Microsoft Azure Bot Service registration
- Microsoft Entra ID (Azure AD) application registration

## Sample

Check out the [Samples.BotEcho](samples/Samples.BotEcho/) directory for a complete working example.

## Building from Source

```bash
# Clone the repository
git clone https://github.com/agnte/Rido.BFLite.git
cd Rido.BFLite

# Build the solution
dotnet build

# Run tests
dotnet test
```

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## License

This project is open source. Please check the repository for license information.

## Related Projects

- [Microsoft Bot Framework](https://github.com/microsoft/botframework-sdk)
- [Bot Framework Composer](https://github.com/microsoft/BotFramework-Composer)
