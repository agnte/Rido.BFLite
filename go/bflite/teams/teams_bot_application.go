// Package teams provides Teams-specific bot functionality.
package teams

import (
	"context"
	"log"
	"net/http"

	"github.com/agnte/Rido.BFLite/go/bflite"
	"github.com/agnte/Rido.BFLite/go/bflite/schema"
	"github.com/agnte/Rido.BFLite/go/bflite/teams/handlers"
	teamsschema "github.com/agnte/Rido.BFLite/go/bflite/teams/schema"
)

// Config holds configuration for the TeamsBotApplication.
type Config struct {
	ClientID     string
	ClientSecret string
	TenantID     string
}

// Context wraps the handler context with Teams-specific functionality.
type Context struct {
	Activity       *teamsschema.TeamsActivity
	BotApplication *TeamsBotApplication
}

// SendActivity sends a text message reply to the current conversation.
func (c *Context) SendActivity(ctx context.Context, text string) (string, error) {
	reply := c.Activity.CreateReplyActivity(text)
	return c.BotApplication.bot.SendActivity(ctx, reply)
}

// TeamsBotApplication extends BotApplication with Teams-specific handlers.
type TeamsBotApplication struct {
	bot    *bflite.BotApplication
	config Config
	logger *log.Logger

	// Handler properties
	OnMessage            handlers.MessageHandler
	OnMessageReaction    handlers.MessageReactionHandler
	OnInstallationUpdate handlers.InstallationUpdateHandler
	OnConversationUpdate handlers.ConversationUpdateHandler
}

// NewTeamsBotApplication creates a new TeamsBotApplication.
func NewTeamsBotApplication(config Config) *TeamsBotApplication {
	// Create token function for Bot Framework authentication
	tokenFunc := createTokenFunc(config)

	bot := bflite.NewBotApplication(tokenFunc)
	app := &TeamsBotApplication{
		bot:    bot,
		config: config,
		logger: log.Default(),
	}

	// Set up the OnActivity callback to dispatch to appropriate handlers
	// FR-007: System MUST invoke appropriate handlers based on activity type
	bot.OnActivity = func(ctx context.Context, activity *schema.Activity) error {
		return app.dispatchActivity(ctx, activity)
	}

	return app
}

// dispatchActivity routes activities to the appropriate handler.
func (t *TeamsBotApplication) dispatchActivity(ctx context.Context, activity *schema.Activity) error {
	// Convert to TeamsActivity
	teamsActivity := teamsschema.FromActivity(activity)

	// Create handler context
	handlerCtx := &Context{
		Activity:       teamsActivity,
		BotApplication: t,
	}

	// Create the generic context for handlers package
	genericCtx := handlers.NewContext(teamsActivity, t.bot)

	if t.logger != nil {
		t.logger.Printf("New activity received of type %s from %s", activity.Type, getFromID(activity))
	}

	// Dispatch based on activity type
	switch activity.Type {
	case teamsschema.ActivityTypes.Message:
		if t.OnMessage != nil {
			return t.OnMessage(ctx, genericCtx)
		}
	case teamsschema.ActivityTypes.MessageReaction:
		if t.OnMessageReaction != nil {
			args := handlers.NewMessageReactionArgs(teamsActivity)
			return t.OnMessageReaction(ctx, args, genericCtx)
		}
	case teamsschema.ActivityTypes.InstallationUpdate:
		if t.OnInstallationUpdate != nil {
			args := handlers.NewInstallationUpdateArgs(teamsActivity)
			return t.OnInstallationUpdate(ctx, args, genericCtx)
		}
	case teamsschema.ActivityTypes.ConversationUpdate:
		if t.OnConversationUpdate != nil {
			args := handlers.NewConversationUpdateArgs(teamsActivity)
			return t.OnConversationUpdate(ctx, args, genericCtx)
		}
	}

	// No handler registered for this activity type, ignore
	_ = handlerCtx // Use to prevent unused variable warning
	return nil
}

// getFromID safely extracts the From ID from an activity.
func getFromID(activity *schema.Activity) string {
	if activity.From != nil {
		return activity.From.ID
	}
	return ""
}

// Use registers a middleware component.
func (t *TeamsBotApplication) Use(middleware bflite.ITurnMiddleware) {
	t.bot.Use(middleware)
}

// SetDebugLogging enables or disables debug logging.
func (t *TeamsBotApplication) SetDebugLogging(enabled bool) {
	t.bot.SetDebugLogging(enabled)
}

// SetLogger sets a custom logger.
func (t *TeamsBotApplication) SetLogger(logger *log.Logger) {
	t.logger = logger
	t.bot.SetLogger(logger)
}

// SendActivity sends an activity directly.
func (t *TeamsBotApplication) SendActivity(ctx context.Context, activity interface{}) (string, error) {
	return t.bot.SendActivity(ctx, activity)
}

// HTTPHandler returns an http.Handler for processing Bot Framework requests.
func (t *TeamsBotApplication) HTTPHandler() http.Handler {
	return t.bot.HTTPHandler()
}

// Listen starts the HTTP server on the specified address.
func (t *TeamsBotApplication) Listen(addr string) error {
	return t.bot.Listen(addr, "/api/messages")
}
