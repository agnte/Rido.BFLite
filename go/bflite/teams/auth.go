package teams

import (
	"context"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"net/url"
	"strings"
	"sync"
	"time"
)

// tokenRefreshBufferSeconds is the number of seconds before token expiry to refresh.
const tokenRefreshBufferSeconds = 300 // 5 minutes

// tokenCache stores cached OAuth tokens.
type tokenCache struct {
	mu        sync.RWMutex
	token     string
	expiresAt time.Time
}

// cachedToken is the global token cache.
var cachedToken = &tokenCache{}

// tokenResponse represents the OAuth token response.
type tokenResponse struct {
	AccessToken string `json:"access_token"`
	TokenType   string `json:"token_type"`
	ExpiresIn   int    `json:"expires_in"`
}

// createTokenFunc creates a token function for Bot Framework authentication.
// FR-004: System MUST authenticate outbound requests using OAuth 2.0 client credentials flow.
func createTokenFunc(config Config) func(ctx context.Context) (string, error) {
	return func(ctx context.Context) (string, error) {
		// Check cache first
		cachedToken.mu.RLock()
		if cachedToken.token != "" && time.Now().Before(cachedToken.expiresAt) {
			token := cachedToken.token
			cachedToken.mu.RUnlock()
			return token, nil
		}
		cachedToken.mu.RUnlock()

		// Acquire write lock and check again (double-checked locking)
		cachedToken.mu.Lock()
		defer cachedToken.mu.Unlock()

		if cachedToken.token != "" && time.Now().Before(cachedToken.expiresAt) {
			return cachedToken.token, nil
		}

		// Get new token
		token, expiresIn, err := acquireToken(ctx, config)
		if err != nil {
			return "", err
		}

		// Cache token with buffer (refresh before expiry)
		cachedToken.token = token
		cachedToken.expiresAt = time.Now().Add(time.Duration(expiresIn-tokenRefreshBufferSeconds) * time.Second)

		return token, nil
	}
}

// acquireToken acquires a new OAuth token from Microsoft identity platform.
func acquireToken(ctx context.Context, config Config) (string, int, error) {
	tenantID := config.TenantID
	if tenantID == "" {
		tenantID = "botframework.com"
	}

	// Token endpoint: https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token
	tokenURL := fmt.Sprintf("https://login.microsoftonline.com/%s/oauth2/v2.0/token", tenantID)

	// Build form data
	data := url.Values{}
	data.Set("grant_type", "client_credentials")
	data.Set("client_id", config.ClientID)
	data.Set("client_secret", config.ClientSecret)
	data.Set("scope", "https://api.botframework.com/.default")

	// Create request
	req, err := http.NewRequestWithContext(ctx, http.MethodPost, tokenURL, strings.NewReader(data.Encode()))
	if err != nil {
		return "", 0, fmt.Errorf("failed to create token request: %w", err)
	}
	req.Header.Set("Content-Type", "application/x-www-form-urlencoded")

	// Send request
	client := &http.Client{Timeout: 30 * time.Second}
	resp, err := client.Do(req)
	if err != nil {
		return "", 0, fmt.Errorf("failed to send token request: %w", err)
	}
	defer resp.Body.Close()

	body, err := io.ReadAll(resp.Body)
	if err != nil {
		return "", 0, fmt.Errorf("failed to read token response: %w", err)
	}

	if resp.StatusCode != http.StatusOK {
		return "", 0, fmt.Errorf("token request failed: %d - %s", resp.StatusCode, string(body))
	}

	var tokenResp tokenResponse
	if err := json.Unmarshal(body, &tokenResp); err != nil {
		return "", 0, fmt.Errorf("failed to parse token response: %w", err)
	}

	return tokenResp.AccessToken, tokenResp.ExpiresIn, nil
}
