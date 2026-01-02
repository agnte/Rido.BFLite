# Rido.BFLite - Node.js/TypeScript Port

A lightweight library for building Microsoft Bot Framework bots with minimal overhead and maximum simplicity - now available for Node.js and TypeScript!

## Overview

This is the Node.js/TypeScript port of the Rido.BFLite library, maintaining the lightweight, minimal API philosophy of the original .NET implementation. It provides a streamlined API for creating Bot Framework bots without the complexity of the full Bot Framework SDK.

## Features

- 🚀 **Lightweight**: Minimal dependencies and overhead
- 🔒 **Secure**: Built-in Bot Framework authentication and JWT validation using MSAL
- 📦 **TypeScript**: Full TypeScript support with type definitions
- ⚡ **Modern**: Built with modern Node.js and Express
- 🎯 **Simple API**: Event-driven programming model matching the .NET version

## Installation

```bash
npm install
```

## Quick Start

### 1. Configuration

Copy `.env.sample` to `.env` and configure your bot credentials:

```bash
cp .env.sample .env
```

Edit `.env` with your bot credentials:

```env
CLIENT_ID=your-bot-app-id
CLIENT_SECRET=your-bot-app-secret
TENANT_ID=your-tenant-id
PORT=3978
```

### 2. Simple Echo Bot

Create a basic echo bot in just a few lines:

```typescript
import express from 'express';
import { BotApplication, createReplyActivity } from '@rido/bflite';

const app = express();
app.use(express.json());

const bot = new BotApplication();

bot.onMessage = async (activity) => {
  const reply = createReplyActivity(activity, `You said: ${activity.text}`);
  await bot.sendActivity(reply);
};

// Mount the bot router at /api/messages
app.use('/api/messages', bot.createRouter());

const PORT = process.env.PORT || 3978;
app.listen(PORT, () => {
  console.log(`Bot is running on port ${PORT}`);
});
```

## API Reference

### BotApplication

The main class for building bots.

#### Event Handlers

- **`onActivity`**: Called for all incoming activities
- **`onMessage`**: Called for message activities
- **`onConversationUpdate`**: Called when members are added or removed
- **`onMessageReaction`**: Called when reactions are added or removed
- **`onInstallationUpdate`**: Called when the bot is installed or uninstalled

#### Methods

- **`sendActivity(activity: Activity): Promise<string>`**: Sends an activity
- **`reply(activity: Activity, text: string): Promise<string>`**: Sends a reply to an activity
- **`createRouter(): Router`**: Creates an Express router with JWT middleware (mount at your desired path)

### Schema Types

#### Activity

```typescript
interface Activity<TChannelData = ChannelData> {
  type: string;
  channelId?: string;
  text?: string;
  id?: string;
  serviceUrl?: string;
  replyToId?: string;
  channelData?: TChannelData;
  from?: ConversationAccount;
  recipient?: ConversationAccount;
  conversation?: Conversation;
  entities?: any[];
}
```

#### Helper Functions

- **`createActivity(type?: string, text?: string): Activity`**: Creates a new activity
- **`createReplyActivity(activity: Activity, text?: string): Activity`**: Creates a reply activity
- **`parseActivity(json: string): Activity`**: Parses an activity from JSON
- **`serializeActivity(activity: Activity): string`**: Serializes an activity to JSON

### Authentication

#### MsalTokenProvider

Handles token acquisition using MSAL:

```typescript
const config = loadAuthConfigFromEnv();
const tokenProvider = new MsalTokenProvider(config);
const token = await tokenProvider.getAccessToken();
```

#### JWT Middleware

Express middleware for validating Bot Framework JWT tokens:

```typescript
import { authorizeJWT } from '@rido/bflite';

app.use(authorizeJWT({ 
  audience: process.env.CLIENT_ID 
}));
```

## Samples

### Echo Bot

A simple bot that echoes back messages:

```bash
npm run build
node dist/samples/echoBot.js
```

### Teams Bot

An advanced bot with Teams-specific features, command handling, and proactive messaging:

```bash
npm run build
node dist/samples/teamsBotApp.js
```

Features:
- Command handling (`/help`, `/time`, `/info`)
- Conversation update notifications
- Message reaction handling
- Proactive messaging via `/api/notify` endpoint

## Building from Source

```bash
# Install dependencies
npm install

# Build TypeScript
npm run build

# Run echo bot sample
npm start
```

## Project Structure

```
node/
├── src/
│   ├── schema/         # Activity and conversation schemas
│   ├── auth/           # Authentication (MSAL, JWT)
│   ├── clients/        # Bot Framework clients
│   ├── botApplication.ts
│   └── index.ts
├── samples/
│   ├── echoBot.ts      # Simple echo bot
│   └── teamsBotApp.ts  # Advanced Teams bot
├── package.json
├── tsconfig.json
└── README.md
```

## Architecture

The library consists of:

- **BotApplication**: Main class for bot applications with event handlers
- **ConversationClient**: HTTP client for sending activities to Bot Framework
- **MsalTokenProvider**: Token provider using MSAL for Bot Framework authentication
- **JWT Middleware**: Express middleware for validating incoming Bot Framework requests
- **Activity Schema**: Strongly-typed TypeScript interfaces for Bot Framework activities

## Requirements

- Node.js 18.x or later
- Microsoft Azure Bot Service registration
- Microsoft Entra ID (Azure AD) application registration

## Security Considerations

### JWT Authentication
The library includes built-in JWT validation middleware that verifies all incoming Bot Framework requests. This ensures that only authenticated requests from the Bot Framework service are processed.

### Rate Limiting
For production deployments, consider implementing rate limiting at the infrastructure level (e.g., using Azure API Management, Azure Front Door, or Express rate-limiting middleware). The Bot Framework infrastructure typically handles rate limiting, but additional application-level protection may be beneficial depending on your deployment scenario.

Example using express-rate-limit:
```typescript
import rateLimit from 'express-rate-limit';

const limiter = rateLimit({
  windowMs: 15 * 60 * 1000, // 15 minutes
  max: 100 // limit each IP to 100 requests per windowMs
});

app.use('/api/messages', limiter, bot.createRouter());
```

## Differences from .NET Version

While maintaining API compatibility, this Node.js port has some differences:

1. **Async/Await**: Uses JavaScript Promises instead of .NET Tasks
2. **Express Integration**: Uses Express.js instead of ASP.NET Core
3. **MSAL Node**: Uses `@azure/msal-node` instead of Microsoft.Identity.Web
4. **JWT Validation**: Uses `jsonwebtoken` and `jwks-rsa` packages

## Related Projects

- [Original .NET Implementation](../)
- [Microsoft Bot Framework](https://github.com/microsoft/botframework-sdk)
- [Bot Framework Documentation](https://docs.microsoft.com/en-us/azure/bot-service/)

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## License

This project follows the same license as the parent project. Please check the repository for license information.
