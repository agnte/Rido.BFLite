package bflite

import (
	"context"
	"encoding/json"
	"fmt"
	"io"
	"log"
	"net/http"

	"github.com/agnte/Rido.BFLite/go/bflite/schema"
)

// DefaultBotEndpointPath is the default path for the bot endpoint.
const DefaultBotEndpointPath = "/api/messages"

// BotHandlerError wraps handler exceptions with the activity that caused the error.
// FR-010: System MUST wrap handler exceptions in BotHandlerException.
type BotHandlerError struct {
	Message  string
	Activity *schema.Activity
	Err      error
}

func (e *BotHandlerError) Error() string {
	return fmt.Sprintf("%s: %v", e.Message, e.Err)
}

func (e *BotHandlerError) Unwrap() error {
	return e.Err
}

// NextDelegate is called to continue the middleware pipeline.
type NextDelegate func(ctx context.Context) error

// ITurnMiddleware defines the interface for middleware components.
// FR-008: System MUST support middleware that executes in registration order.
type ITurnMiddleware interface {
	OnTurn(ctx context.Context, bot *BotApplication, activity *schema.Activity, next NextDelegate) error
}

// ActivityCallback is the callback invoked when an activity is received.
type ActivityCallback func(ctx context.Context, activity *schema.Activity) error

// BotApplication is the main bot class that handles incoming activities.
type BotApplication struct {
	conversationClient *ConversationClient
	middlewares        []ITurnMiddleware
	OnActivity         ActivityCallback
	logger             *log.Logger
	debugLogging       bool
}

// NewBotApplication creates a new BotApplication.
func NewBotApplication(tokenFunc func(ctx context.Context) (string, error)) *BotApplication {
	return &BotApplication{
		conversationClient: NewConversationClient(tokenFunc),
		middlewares:        make([]ITurnMiddleware, 0),
		logger:             log.Default(),
	}
}

// SetDebugLogging enables or disables debug logging.
func (b *BotApplication) SetDebugLogging(enabled bool) {
	b.debugLogging = enabled
	b.conversationClient.SetDebugLogging(enabled)
}

// SetLogger sets a custom logger.
func (b *BotApplication) SetLogger(logger *log.Logger) {
	b.logger = logger
	b.conversationClient.SetLogger(logger)
}

// Use registers a middleware component.
// FR-008: Middleware executes in registration order.
func (b *BotApplication) Use(middleware ITurnMiddleware) {
	b.middlewares = append(b.middlewares, middleware)
}

// ProcessActivity processes an incoming activity through the middleware pipeline and handlers.
func (b *BotApplication) ProcessActivity(ctx context.Context, activity *schema.Activity) error {
	if b.debugLogging {
		jsonBytes, _ := activity.ToJSON()
		b.logger.Printf("Received activity: %s", string(jsonBytes))
	}

	// Run middleware pipeline
	err := b.runPipeline(ctx, activity, 0)
	if err != nil {
		b.logger.Printf("Error processing activity %s %s: %v", activity.Type, activity.ID, err)
		return &BotHandlerError{
			Message:  "Error processing activity",
			Activity: activity,
			Err:      err,
		}
	}

	if b.debugLogging {
		b.logger.Printf("Finished processing activity %s %s", activity.Type, activity.ID)
	}

	return nil
}

// runPipeline executes the middleware pipeline recursively.
func (b *BotApplication) runPipeline(ctx context.Context, activity *schema.Activity, index int) error {
	if index >= len(b.middlewares) {
		// End of middleware chain, invoke the OnActivity callback
		if b.OnActivity != nil {
			return b.OnActivity(ctx, activity)
		}
		return nil
	}

	middleware := b.middlewares[index]
	return middleware.OnTurn(ctx, b, activity, func(nextCtx context.Context) error {
		return b.runPipeline(nextCtx, activity, index+1)
	})
}

// SendActivity sends an activity to the Bot Framework service.
func (b *BotApplication) SendActivity(ctx context.Context, activity interface{}) (string, error) {
	// Handle both *schema.Activity and pointer types
	switch a := activity.(type) {
	case *schema.Activity:
		return b.conversationClient.SendActivity(ctx, a)
	default:
		// Try to convert through JSON for other types
		jsonBytes, err := json.Marshal(activity)
		if err != nil {
			return "", fmt.Errorf("failed to marshal activity: %w", err)
		}
		var coreActivity schema.Activity
		if err := json.Unmarshal(jsonBytes, &coreActivity); err != nil {
			return "", fmt.Errorf("failed to unmarshal to core activity: %w", err)
		}
		return b.conversationClient.SendActivity(ctx, &coreActivity)
	}
}

// HTTPHandler returns an http.Handler that processes incoming Bot Framework requests.
// FR-002: System MUST expose a POST endpoint that accepts Activity JSON and requires JWT authentication.
func (b *BotApplication) HTTPHandler() http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodPost {
			http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
			return
		}

		// Read request body
		body, err := io.ReadAll(r.Body)
		if err != nil {
			http.Error(w, "Failed to read request body", http.StatusBadRequest)
			return
		}
		defer r.Body.Close()

		// Parse activity
		var activity schema.Activity
		if err := json.Unmarshal(body, &activity); err != nil {
			http.Error(w, "Invalid activity JSON", http.StatusBadRequest)
			return
		}

		// Process activity
		ctx := r.Context()
		if err := b.ProcessActivity(ctx, &activity); err != nil {
			b.logger.Printf("Error processing activity: %v", err)
			http.Error(w, "Internal server error", http.StatusInternalServerError)
			return
		}

		// Return success with activity ID
		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		response := map[string]string{"id": activity.ID}
		json.NewEncoder(w).Encode(response)
	})
}

// Listen starts the HTTP server on the specified address.
func (b *BotApplication) Listen(addr string, path string) error {
	if path == "" {
		path = DefaultBotEndpointPath
	}
	b.logger.Printf("Bot listening on %s%s", addr, path)
	http.Handle(path, b.HTTPHandler())
	return http.ListenAndServe(addr, nil)
}
