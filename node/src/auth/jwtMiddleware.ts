import { Request, Response, NextFunction } from 'express';
import * as jwt from 'jsonwebtoken';
import { JwksClient } from 'jwks-rsa';

/**
 * JWT validation options
 */
export interface JwtValidationOptions {
  audience: string;
  validIssuers?: string[];
}

/**
 * Creates JWKS clients for different issuers
 */
const jwksClients = new Map<string, JwksClient>();

/**
 * Gets the JWKS client for a given issuer
 */
function getJwksClient(issuer: string): JwksClient {
  let client = jwksClients.get(issuer);
  
  if (!client) {
    let jwksUri: string;
    
    if (issuer === 'https://api.botframework.com') {
      jwksUri = 'https://login.botframework.com/v1/.well-known/keys';
    } else {
      // Extract tenant ID from issuer if it's an Azure AD issuer
      const tenantMatch = issuer.match(/\/([0-9a-f-]+)\/?/i);
      const tenantId = tenantMatch ? tenantMatch[1] : 'common';
      jwksUri = `https://login.microsoftonline.com/${tenantId}/discovery/v2.0/keys`;
    }
    
    client = new JwksClient({
      jwksUri,
      cache: true,
      cacheMaxAge: 86400000, // 24 hours
      rateLimit: true
    });
    
    jwksClients.set(issuer, client);
  }
  
  return client;
}

/**
 * Creates a function to get the signing key from JWKS
 */
function createSigningKeyGetter(token: string) {
  return (header: jwt.JwtHeader, callback: jwt.SigningKeyCallback): void => {
    if (!header.kid) {
      return callback(new Error('No kid in token header'));
    }

    // Decode the token to get the issuer without validation
    const decoded = jwt.decode(token, { complete: true }) as jwt.Jwt | null;
    if (!decoded || typeof decoded.payload === 'string') {
      return callback(new Error('Invalid token'));
    }

    const issuer = decoded.payload.iss;
    if (!issuer) {
      return callback(new Error('No issuer in token'));
    }

    const client = getJwksClient(issuer);
    
    client.getSigningKey(header.kid, (err, key) => {
      if (err) {
        return callback(err);
      }
      const signingKey = key?.getPublicKey();
      callback(null, signingKey);
    });
  };
}

/**
 * Express middleware for validating Bot Framework JWT tokens
 */
export function authorizeJWT(options: JwtValidationOptions) {
  const validIssuers = options.validIssuers || [
    'https://api.botframework.com',
    `https://sts.windows.net/${process.env.TENANT_ID || 'common'}/`,
    `https://login.microsoftonline.com/${process.env.TENANT_ID || 'common'}/v2.0`
  ];

  return async (req: Request, res: Response, next: NextFunction): Promise<void> => {
    try {
      const authHeader = req.headers.authorization;

      if (!authHeader) {
        res.status(401).json({ error: 'No authorization header' });
        return;
      }

      const parts = authHeader.split(' ');
      if (parts.length !== 2 || parts[0] !== 'Bearer') {
        res.status(401).json({ error: 'Invalid authorization header format' });
        return;
      }

      const token = parts[1];

      // Verify the token
      jwt.verify(
        token,
        createSigningKeyGetter(token),
        {
          audience: options.audience,
          issuer: validIssuers.length === 1 ? validIssuers[0] : validIssuers as [string, ...string[]],
          algorithms: ['RS256']
        },
        (err: jwt.VerifyErrors | null, decoded: any) => {
          if (err) {
            console.error('JWT verification failed:', err.message);
            res.status(401).json({ error: 'Invalid token', details: err.message });
            return;
          }

          // Attach decoded token to request
          (req as any).user = decoded;
          next();
        }
      );
    } catch (error) {
      console.error('JWT middleware error:', error);
      res.status(500).json({ error: 'Internal server error' });
    }
  };
}
