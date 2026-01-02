/**
 * Base ChannelData interface matching the .NET schema
 */
export interface ChannelData {
  clientActivityID?: string;
  [key: string]: any; // Extension data
}

/**
 * Teams-specific channel data
 */
export interface TeamsChannelData extends ChannelData {
  teamsChannelId?: string;
  teamsTeamId?: string;
  channel?: TeamsChannel;
  team?: Team;
  tenant?: TeamsChannelDataTenant;
  settings?: TeamsChannelDataSettings;
}

/**
 * Teams channel information
 */
export interface TeamsChannel {
  id?: string;
  name?: string;
}

/**
 * Teams team information
 */
export interface Team {
  id?: string;
  name?: string;
}

/**
 * Teams tenant information
 */
export interface TeamsChannelDataTenant {
  id?: string;
}

/**
 * Teams channel data settings
 */
export interface TeamsChannelDataSettings {
  selectedChannel?: {
    id?: string;
  };
}
