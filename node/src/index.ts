// Main exports
export { BotApplication } from './botApplication';
export type {
  ActivityHandler,
  MessageHandler,
  ConversationUpdateHandler,
  MessageReactionHandler,
  InstallationUpdateHandler
} from './botApplication';

// Schema exports
export type { Activity, ExtendedPropertiesDictionary } from './schema/activity';
export { createActivity, createReplyActivity, parseActivity, serializeActivity } from './schema/activity';
export type { Conversation, ConversationAccount } from './schema/conversation';
export type {
  ChannelData,
  TeamsChannelData,
  TeamsChannel,
  Team,
  TeamsChannelDataTenant,
  TeamsChannelDataSettings
} from './schema/channelData';

// Auth exports
export { MsalTokenProvider, loadAuthConfigFromEnv } from './auth/tokenProvider';
export type { AuthConfig } from './auth/tokenProvider';
export { authorizeJWT } from './auth/jwtMiddleware';
export type { JwtValidationOptions } from './auth/jwtMiddleware';

// Client exports
export { ConversationClient } from './clients/conversationClient';
export type { SendActivityResponse } from './clients/conversationClient';
