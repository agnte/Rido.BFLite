import axios, { AxiosInstance } from 'axios';
import { Activity } from '../schema/activity';
import { MsalTokenProvider } from '../auth/tokenProvider';

/**
 * Response from sending an activity
 */
export interface SendActivityResponse {
  id: string;
}

/**
 * Client for sending activities to the Bot Framework Connector Service
 */
export class ConversationClient {
  private httpClient: AxiosInstance;
  private tokenProvider: MsalTokenProvider;

  constructor(tokenProvider: MsalTokenProvider) {
    this.tokenProvider = tokenProvider;
    this.httpClient = axios.create({
      headers: {
        'Content-Type': 'application/json'
      }
    });
  }

  /**
   * Sends an activity to the Bot Framework Connector Service
   * @param activity - The activity to send
   * @returns The activity ID
   */
  async sendActivity(activity: Activity): Promise<string> {
    // Skip trace activities (matching .NET behavior)
    if (activity.type === 'trace') {
      console.log(`Skipping trace activity ${activity.id}`);
      return '';
    }

    // Skip invoke activities (matching .NET behavior)
    if (activity.type.toLowerCase().includes('invoke')) {
      console.log(`Skipping invoke activity ${activity.id}`);
      return '';
    }

    if (!activity.serviceUrl || !activity.conversation?.id) {
      throw new Error('Activity must have serviceUrl and conversation.id');
    }

    const url = `${activity.serviceUrl}/v3/conversations/${activity.conversation.id}/activities/`;
    
    // Get access token
    const token = await this.tokenProvider.getAccessToken();

    try {
      const response = await this.httpClient.post<SendActivityResponse>(
        url,
        activity,
        {
          headers: {
            'Authorization': `Bearer ${token}`
          }
        }
      );

      console.log(`Activity sent successfully. Status: ${response.status}`);
      return response.data.id || '';
    } catch (error) {
      if (axios.isAxiosError(error)) {
        const status = error.response?.status;
        const data = error.response?.data;
        throw new Error(`Error sending activity: ${status} - ${JSON.stringify(data)}`);
      }
      throw error;
    }
  }
}
