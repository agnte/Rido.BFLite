import { ExtendedPropertiesDictionary } from './activity';

/**
 * ConversationAccount interface matching the .NET schema
 */
export interface ConversationAccount {
  id?: string;
  name?: string;
  [key: string]: any; // Extension data
}

/**
 * Conversation interface matching the .NET schema
 */
export interface Conversation {
  id?: string;
  [key: string]: any; // Extension data
}
