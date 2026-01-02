/**
 * Simple echo bot sample demonstrating basic usage of the BotApplication class
 */
import express from 'express';
import * as dotenv from 'dotenv';
import { BotApplication, createReplyActivity } from '../src';

// Load environment variables
dotenv.config();

// Create Express app
const app = express();
app.use(express.json());

// Create bot application
const bot = new BotApplication();

// Handle incoming messages
bot.onMessage = async (activity) => {
  console.log(`Received message: ${activity.text}`);
  
  const reply = createReplyActivity(activity, `You said: ${activity.text}`);
  await bot.sendActivity(reply);
};

// Health check endpoint
app.get('/health', (req, res) => {
  res.json({ status: 'healthy', timestamp: new Date().toISOString() });
});

// Mount the bot router at /api/messages
app.use('/api/messages', bot.createRouter());

// Start the server
const PORT = process.env.PORT || 3978;
app.listen(PORT, () => {
  console.log(`Echo bot is running on port ${PORT}`);
  console.log(`Bot endpoint: http://localhost:${PORT}/api/messages`);
  console.log(`Health check: http://localhost:${PORT}/health`);
});
