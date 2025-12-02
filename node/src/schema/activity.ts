import { Conversation } from './conversation';
import { ConversationAccount } from './conversation';
import { ChannelData } from './channelData';

/**
 * Extended properties dictionary for Activity
 */
export interface ExtendedPropertiesDictionary {
  [key: string]: any;
}

/**
 * Activity interface matching the .NET Activity schema
 */
export interface Activity<TChannelData extends ChannelData = ChannelData> {
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
  [key: string]: any; // Extension data
}

/**
 * Creates a new Activity object
 */
export function createActivity(type: string = 'message', text?: string): Activity {
  return {
    type,
    text
  };
}

/**
 * Creates a reply activity from an incoming activity
 */
export function createReplyActivity<TChannelData extends ChannelData = ChannelData>(
  activity: Activity<TChannelData>,
  text: string = ''
): Activity {
  return {
    type: 'message',
    channelId: activity.channelId,
    serviceUrl: activity.serviceUrl,
    conversation: activity.conversation,
    from: activity.recipient,
    recipient: activity.from,
    replyToId: activity.id,
    text
  };
}

/**
 * Parses an Activity from a JSON string
 */
export function parseActivity<TChannelData extends ChannelData = ChannelData>(
  json: string
): Activity<TChannelData> {
  return JSON.parse(json) as Activity<TChannelData>;
}

/**
 * Serializes an Activity to a JSON string
 */
export function serializeActivity<TChannelData extends ChannelData = ChannelData>(
  activity: Activity<TChannelData>
): string {
  return JSON.stringify(activity, (key, value) => {
    // Filter out undefined values to match the .NET behavior
    if (value === undefined) {
      return undefined;
    }
    return value;
  }, 2);
}
