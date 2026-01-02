import { ConfidentialClientApplication, Configuration, AuthenticationResult } from '@azure/msal-node';
import * as dotenv from 'dotenv';

/**
 * Configuration for authentication
 */
export interface AuthConfig {
  clientId: string;
  clientSecret: string;
  tenantId: string;
}

/**
 * Token cache entry
 */
interface TokenCacheEntry {
  token: string;
  expiresAt: number;
}

/**
 * MSAL-based token provider for Bot Framework authentication
 */
export class MsalTokenProvider {
  private cca: ConfidentialClientApplication;
  private tokenCache: Map<string, TokenCacheEntry> = new Map();
  private readonly DEFAULT_SCOPE = 'https://api.botframework.com/.default';

  constructor(config: AuthConfig) {
    const msalConfig: Configuration = {
      auth: {
        clientId: config.clientId,
        authority: `https://login.microsoftonline.com/${config.tenantId}`,
        clientSecret: config.clientSecret
      }
    };

    this.cca = new ConfidentialClientApplication(msalConfig);
  }

  /**
   * Acquires an access token for the specified scope
   * @param scope - The scope to request. Defaults to Bot Framework scope.
   * @returns The access token
   */
  async getAccessToken(scope: string = this.DEFAULT_SCOPE): Promise<string> {
    // Check cache first
    const cached = this.tokenCache.get(scope);
    if (cached && cached.expiresAt > Date.now() + 60000) { // 1 minute buffer
      return cached.token;
    }

    // Acquire new token
    const result: AuthenticationResult | null = await this.cca.acquireTokenByClientCredential({
      scopes: [scope]
    });

    if (!result || !result.accessToken) {
      throw new Error('Failed to acquire access token');
    }

    // Cache the token
    const expiresAt = result.expiresOn ? result.expiresOn.getTime() : Date.now() + 3600000; // Default 1 hour
    this.tokenCache.set(scope, {
      token: result.accessToken,
      expiresAt
    });

    return result.accessToken;
  }

  /**
   * Clears the token cache
   */
  clearCache(): void {
    this.tokenCache.clear();
  }
}

/**
 * Loads authentication configuration from environment variables
 * @returns AuthConfig object
 */
export function loadAuthConfigFromEnv(): AuthConfig {
  dotenv.config();

  const clientId = process.env.CLIENT_ID;
  const clientSecret = process.env.CLIENT_SECRET;
  const tenantId = process.env.TENANT_ID;

  if (!clientId || !clientSecret || !tenantId) {
    throw new Error('Missing required environment variables: CLIENT_ID, CLIENT_SECRET, or TENANT_ID');
  }

  return {
    clientId,
    clientSecret,
    tenantId
  };
}
