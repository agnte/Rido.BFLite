/**
 * Advanced Teams bot sample demonstrating Teams-specific features,
 * proactive messaging, and command handling
 */
import express from 'express';
import * as dotenv from 'dotenv';
import { BotApplication, createReplyActivity, Activity } from '../src';

// Load environment variables
dotenv.config();

// Create Express app
const app = express();
app.use(express.json());

// Create bot application
const bot = new BotApplication();

// Store the last activity for proactive messaging
let lastActivity: Activity | null = null;

// Handle incoming messages with command support
bot.onMessage = async (activity) => {
  console.log(`Received message from ${activity.from?.name}: ${activity.text}`);
  
  // Store activity for proactive messaging
  lastActivity = activity;

  const text = activity.text?.trim().toLowerCase() || '';

  // Command handling
  if (text.startsWith('/')) {
    await handleCommand(activity, text);
    return;
  }

  // Echo the message with timestamp
  const reply = createReplyActivity(
    activity,
    `You said: ${activity.text}, with ❤️ at ${new Date().toLocaleTimeString()}`
  );
  await bot.sendActivity(reply);
};

// Handle conversation updates (member add/remove)
bot.onConversationUpdate = async (activity) => {
  console.log('Conversation update received');

  const membersAdded = (activity as any).membersAdded || [];
  const membersRemoved = (activity as any).membersRemoved || [];

  let result = '**Members changed**\n\n';

  if (membersAdded.length > 0) {
    result += '**Added:**\n';
    membersAdded.forEach((member: any) => {
      result += `- ${member.name || member.id}\n`;
    });
  }

  if (membersRemoved.length > 0) {
    result += '\n**Removed:**\n';
    membersRemoved.forEach((member: any) => {
      result += `- ${member.name || member.id}\n`;
    });
  }

  const reply = createReplyActivity(activity, result);
  await bot.sendActivity(reply);
};

// Handle message reactions
bot.onMessageReaction = async (activity) => {
  console.log('Message reaction received');

  const reactionsAdded = (activity as any).reactionsAdded || [];
  const reactionsRemoved = (activity as any).reactionsRemoved || [];

  const addedType = reactionsAdded.length > 0 ? reactionsAdded[0].type : 'none';
  const removedType = reactionsRemoved.length > 0 ? reactionsRemoved[0].type : 'none';

  const result = `Reaction received at ${new Date().toLocaleTimeString()}\n` +
    `Added: ${addedType}\n` +
    `Removed: ${removedType}`;

  const reply = createReplyActivity(activity, result);
  await bot.sendActivity(reply);
};

// Handle installation updates (bot installed/uninstalled)
bot.onInstallationUpdate = async (activity) => {
  const action = (activity as any).action || 'unknown';
  const channelData = activity.channelData as any;
  const channelId = channelData?.teamsChannelId || 'unknown';

  console.log(`Installation update: ${action} for channel ${channelId}`);
};

/**
 * Handles bot commands
 */
async function handleCommand(activity: Activity, command: string): Promise<void> {
  switch (command) {
    case '/help':
      await sendHelp(activity);
      break;
    case '/time':
      await sendTime(activity);
      break;
    case '/info':
      await sendInfo(activity);
      break;
    default:
      const reply = createReplyActivity(
        activity,
        `Unknown command: ${command}. Type /help for available commands.`
      );
      await bot.sendActivity(reply);
  }
}

/**
 * Sends help message
 */
async function sendHelp(activity: Activity): Promise<void> {
  const helpText = `**Available Commands:**\n\n` +
    `- **/help** - Show this help message\n` +
    `- **/time** - Show current time\n` +
    `- **/info** - Show bot and conversation info\n`;

  const reply = createReplyActivity(activity, helpText);
  await bot.sendActivity(reply);
}

/**
 * Sends current time
 */
async function sendTime(activity: Activity): Promise<void> {
  const now = new Date();
  const timeText = `**Current time:**\n\n` +
    `📅 Date: ${now.toLocaleDateString()}\n` +
    `🕐 Time: ${now.toLocaleTimeString()}\n` +
    `🌍 UTC: ${now.toUTCString()}`;

  const reply = createReplyActivity(activity, timeText);
  await bot.sendActivity(reply);
}

/**
 * Sends bot and conversation info
 */
async function sendInfo(activity: Activity): Promise<void> {
  const infoText = `**Bot Information:**\n\n` +
    `- **Conversation ID:** ${activity.conversation?.id}\n` +
    `- **Channel ID:** ${activity.channelId}\n` +
    `- **From:** ${activity.from?.name} (${activity.from?.id})\n` +
    `- **Service URL:** ${activity.serviceUrl}\n`;

  const reply = createReplyActivity(activity, infoText);
  await bot.sendActivity(reply);
}

// Proactive messaging endpoint
app.get('/api/notify', async (req, res) => {
  try {
    if (!lastActivity) {
      res.status(400).json({ error: 'No conversation history. Send a message to the bot first.' });
      return;
    }

    // Create a proactive message
    const proactiveMessage: Activity = {
      type: 'message',
      conversation: lastActivity.conversation,
      from: lastActivity.recipient,
      recipient: lastActivity.from,
      serviceUrl: lastActivity.serviceUrl,
      text: `🔔 **Proactive notification** sent at ${new Date().toLocaleTimeString()}`
    };

    await bot.sendActivity(proactiveMessage);

    res.json({
      status: 'success',
      message: 'Proactive notification sent',
      conversationId: lastActivity.conversation?.id
    });
  } catch (error) {
    console.error('Error sending proactive message:', error);
    res.status(500).json({ error: 'Failed to send proactive message' });
  }
});

// Health check endpoint
app.get('/health', (req, res) => {
  res.json({
    status: 'healthy',
    timestamp: new Date().toISOString(),
    hasConversation: !!lastActivity
  });
});

// Mount the bot router at /api/messages
app.use('/api/messages', bot.createRouter());

// Start the server
const PORT = process.env.PORT || 3978;
app.listen(PORT, () => {
  console.log(`Teams bot is running on port ${PORT}`);
  console.log(`Bot endpoint: http://localhost:${PORT}/api/messages`);
  console.log(`Proactive notify: http://localhost:${PORT}/api/notify`);
  console.log(`Health check: http://localhost:${PORT}/health`);
  console.log('\nAvailable commands:');
  console.log('  /help - Show help message');
  console.log('  /time - Show current time');
  console.log('  /info - Show bot information');
});
