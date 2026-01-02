// Package main demonstrates a simple echo bot using Rido.BFLite for Go.
// This example follows the pattern from TRANSLATION_SPEC.md.
package main

import (
	"context"
	"fmt"
	"log"
	"os"
	"time"

	"github.com/agnte/Rido.BFLite/go/bflite/teams"
	"github.com/agnte/Rido.BFLite/go/bflite/teams/handlers"
)

func main() {
	// Get configuration from environment variables
	config := teams.Config{
		ClientID:     getEnv("CLIENT_ID", "your-app-id"),
		ClientSecret: getEnv("CLIENT_SECRET", "your-app-secret"),
		TenantID:     getEnv("TENANT_ID", ""),
	}

	// Create Teams bot application
	bot := teams.NewTeamsBotApplication(config)

	// Enable debug logging (optional)
	if os.Getenv("DEBUG") == "true" {
		bot.SetDebugLogging(true)
	}

	// Set up message handler
	// User Story 1 - Echo Bot: Bot receives message and responds with echo
	bot.OnMessage = func(ctx context.Context, c *handlers.Context) error {
		text := c.Activity.Text
		response := fmt.Sprintf("You said: %s, with ❤️ at %s", text, time.Now().Format("15:04:05"))
		_, err := c.SendActivity(ctx, response)
		return err
	}

	// Set up message reaction handler
	// User Story 3 - Handle Message Reactions
	bot.OnMessageReaction = func(ctx context.Context, args *handlers.MessageReactionArgs, c *handlers.Context) error {
		var addedType, removedType string
		if len(args.ReactionsAdded) > 0 {
			addedType = args.ReactionsAdded[0].Type
		}
		if len(args.ReactionsRemoved) > 0 {
			removedType = args.ReactionsRemoved[0].Type
		}

		response := fmt.Sprintf("Reaction received at %s. Added: %s Removed: %s",
			time.Now().Format("15:04:05"), addedType, removedType)
		_, err := c.SendActivity(ctx, response)
		return err
	}

	// Set up installation update handler
	// User Story 5 - Teams Installation Events
	bot.OnInstallationUpdate = func(ctx context.Context, args *handlers.InstallationUpdateArgs, c *handlers.Context) error {
		response := fmt.Sprintf("Installation update event. Action: %s for %s channel",
			args.Action, args.SelectedChannelID)
		_, err := c.SendActivity(ctx, response)
		return err
	}

	// Set up conversation update handler
	// User Story 4 - Conversation Updates
	bot.OnConversationUpdate = func(ctx context.Context, args *handlers.ConversationUpdateArgs, c *handlers.Context) error {
		var result string = "Members changed\n\nAdded:\n"
		for _, member := range args.MembersAdded {
			result += fmt.Sprintf("**%s**\n", member.Name)
		}
		result += "\nRemoved:\n"
		for _, member := range args.MembersRemoved {
			result += fmt.Sprintf("%s\n", member.Name)
		}
		_, err := c.SendActivity(ctx, result)
		return err
	}

	// Start the bot server
	port := getEnv("PORT", "3978")
	addr := ":" + port
	log.Printf("Starting bot on %s", addr)
	if err := bot.Listen(addr); err != nil {
		log.Fatalf("Failed to start bot: %v", err)
	}
}

// getEnv returns the environment variable value or a default.
func getEnv(key, defaultValue string) string {
	if value := os.Getenv(key); value != "" {
		return value
	}
	return defaultValue
}
