import { Request, Response, Router } from 'express';
import { Activity, createReplyActivity } from './schema/activity';
import { ConversationClient } from './clients/conversationClient';
import { MsalTokenProvider, loadAuthConfigFromEnv } from './auth/tokenProvider';
import { authorizeJWT } from './auth/jwtMiddleware';

/**
 * Event handler for activities
 */
export type ActivityHandler = (activity: Activity, cancellationToken?: AbortSignal) => Promise<void>;

/**
 * Event handler for message activities
 */
export type MessageHandler = (activity: Activity, cancellationToken?: AbortSignal) => Promise<void>;

/**
 * Event handler for conversation update activities
 */
export type ConversationUpdateHandler = (activity: Activity, cancellationToken?: AbortSignal) => Promise<void>;

/**
 * Event handler for message reaction activities
 */
export type MessageReactionHandler = (activity: Activity, cancellationToken?: AbortSignal) => Promise<void>;

/**
 * Event handler for installation update activities
 */
export type InstallationUpdateHandler = (activity: Activity, cancellationToken?: AbortSignal) => Promise<void>;

/**
 * Main bot application class matching the .NET API
 */
export class BotApplication {
  private conversationClient?: ConversationClient;
  private tokenProvider?: MsalTokenProvider;

  // Event handlers
  public onActivity?: ActivityHandler;
  public onMessage?: MessageHandler;
  public onConversationUpdate?: ConversationUpdateHandler;
  public onMessageReaction?: MessageReactionHandler;
  public onInstallationUpdate?: InstallationUpdateHandler;

  constructor(tokenProvider?: MsalTokenProvider) {
    if (tokenProvider) {
      this.tokenProvider = tokenProvider;
      this.conversationClient = new ConversationClient(tokenProvider);
    }
  }

  /**
   * Initializes the bot application with environment configuration
   */
  initialize(): void {
    if (!this.tokenProvider) {
      const config = loadAuthConfigFromEnv();
      this.tokenProvider = new MsalTokenProvider(config);
      this.conversationClient = new ConversationClient(this.tokenProvider);
    }
  }

  /**
   * Processes an incoming request
   * @param req - Express request object
   * @param res - Express response object
   */
  async processRequest(req: Request, res: Response): Promise<void> {
    try {
      const activity: Activity = req.body;

      if (!activity || !activity.type) {
        res.status(400).json({ error: 'Invalid activity' });
        return;
      }

      console.log(`Received activity: ${activity.type} ${activity.id}`);

      // Process the activity through handlers
      await this.processActivity(activity);

      res.status(200).json({ status: 'ok' });
    } catch (error) {
      console.error('Error processing request:', error);
      res.status(500).json({ error: 'Internal server error' });
    }
  }

  /**
   * Processes an activity through the appropriate handler
   */
  private async processActivity(activity: Activity, cancellationToken?: AbortSignal): Promise<void> {
    // Call the general activity handler if set
    if (this.onActivity) {
      await this.onActivity(activity, cancellationToken);
    }

    // Call specific handlers based on activity type
    const activityType = activity.type?.toLowerCase();

    if (activityType === 'message' && this.onMessage) {
      await this.onMessage(activity, cancellationToken);
    } else if (activityType === 'conversationupdate' && this.onConversationUpdate) {
      await this.onConversationUpdate(activity, cancellationToken);
    } else if (activityType === 'messagereaction' && this.onMessageReaction) {
      await this.onMessageReaction(activity, cancellationToken);
    } else if (activityType === 'installationupdate' && this.onInstallationUpdate) {
      await this.onInstallationUpdate(activity, cancellationToken);
    }
  }

  /**
   * Sends an activity through the conversation client
   * @param activity - The activity to send
   * @returns The activity ID
   */
  async sendActivity(activity: Activity): Promise<string> {
    if (!this.conversationClient) {
      throw new Error('ConversationClient not initialized. Call initialize() first.');
    }

    return await this.conversationClient.sendActivity(activity);
  }

  /**
   * Helper method to send a reply to an activity
   * @param activity - The activity to reply to
   * @param text - The text of the reply
   * @returns The activity ID
   */
  async reply(activity: Activity, text: string): Promise<string> {
    const replyActivity = createReplyActivity(activity, text);
    return await this.sendActivity(replyActivity);
  }

  /**
   * Creates an Express router with JWT middleware
   * @param path - The path for the bot endpoint (default: '/api/messages')
   * @returns Express router
   */
  createRouter(path: string = '/api/messages'): Router {
    this.initialize();

    const router = Router();
    const clientId = process.env.CLIENT_ID;

    if (!clientId) {
      throw new Error('CLIENT_ID environment variable is required');
    }

    // Add JWT authentication middleware
    router.use(authorizeJWT({ audience: clientId }));

    // Add the bot message handler
    router.post(path, (req, res) => this.processRequest(req, res));

    return router;
  }
}
