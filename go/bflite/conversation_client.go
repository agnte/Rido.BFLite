// Package bflite provides a lightweight library for building Microsoft Bot Framework bots.
package bflite

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"io"
	"log"
	"net/http"

	"github.com/agnte/Rido.BFLite/go/bflite/schema"
)

// ConversationClient handles sending activities to the Bot Framework service.
type ConversationClient struct {
	httpClient   *http.Client
	tokenFunc    func(ctx context.Context) (string, error)
	logger       *log.Logger
	debugLogging bool
}

// NewConversationClient creates a new ConversationClient.
func NewConversationClient(tokenFunc func(ctx context.Context) (string, error)) *ConversationClient {
	return &ConversationClient{
		httpClient: &http.Client{},
		tokenFunc:  tokenFunc,
		logger:     log.Default(),
	}
}

// SetDebugLogging enables or disables debug logging.
func (c *ConversationClient) SetDebugLogging(enabled bool) {
	c.debugLogging = enabled
}

// SetLogger sets a custom logger.
func (c *ConversationClient) SetLogger(logger *log.Logger) {
	c.logger = logger
}

// SendActivity sends an activity to the Bot Framework service.
// FR-006: Sends activities via HTTP POST to {ServiceUrl}v3/conversations/{ConversationId}/activities/
func (c *ConversationClient) SendActivity(ctx context.Context, activity *schema.Activity) (string, error) {
	// Skip trace activities
	if activity.Type == "trace" {
		if c.debugLogging {
			c.logger.Printf("Skipping trace activity %s", activity.ID)
		}
		return "", nil
	}

	// Skip invoke activities
	if activity.Type == "invoke" {
		if c.debugLogging {
			c.logger.Printf("Skipping invoke activity %s", activity.ID)
		}
		return "", nil
	}

	// Build URL: {ServiceUrl}v3/conversations/{ConversationId}/activities/
	if activity.ServiceURL == "" {
		return "", fmt.Errorf("activity ServiceURL is required")
	}
	if activity.Conversation == nil || activity.Conversation.ID == "" {
		return "", fmt.Errorf("activity Conversation.ID is required")
	}

	url := fmt.Sprintf("%sv3/conversations/%s/activities/", activity.ServiceURL, activity.Conversation.ID)

	// Serialize activity
	body, err := json.Marshal(activity)
	if err != nil {
		return "", fmt.Errorf("failed to serialize activity: %w", err)
	}

	if c.debugLogging {
		c.logger.Printf("POST %s\nBody: %s", url, string(body))
	}

	// Create request
	req, err := http.NewRequestWithContext(ctx, http.MethodPost, url, bytes.NewReader(body))
	if err != nil {
		return "", fmt.Errorf("failed to create request: %w", err)
	}
	req.Header.Set("Content-Type", "application/json")

	// Get and set authorization token
	if c.tokenFunc != nil {
		token, err := c.tokenFunc(ctx)
		if err != nil {
			return "", fmt.Errorf("failed to get authorization token: %w", err)
		}
		req.Header.Set("Authorization", "Bearer "+token)
	}

	// Send request
	resp, err := c.httpClient.Do(req)
	if err != nil {
		return "", fmt.Errorf("failed to send activity: %w", err)
	}
	defer resp.Body.Close()

	respBody, err := io.ReadAll(resp.Body)
	if err != nil {
		return "", fmt.Errorf("failed to read response: %w", err)
	}

	if c.debugLogging {
		c.logger.Printf("Response Status: %d, Content: %s", resp.StatusCode, string(respBody))
	}

	if resp.StatusCode < 200 || resp.StatusCode >= 300 {
		return "", fmt.Errorf("error sending activity: %d - %s", resp.StatusCode, string(respBody))
	}

	// Parse response to get activity ID
	var result struct {
		ID string `json:"id"`
	}
	if err := json.Unmarshal(respBody, &result); err != nil {
		// If response is not JSON with ID, return empty string (success but no ID)
		return "", nil
	}

	return result.ID, nil
}
