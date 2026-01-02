package handlers

import (
	"context"
	"encoding/json"
	"testing"

	"github.com/agnte/Rido.BFLite/go/bflite/teams/schema"
)

func TestNewMessageReactionArgs(t *testing.T) {
	jsonStr := `{
		"type": "messageReaction",
		"reactionsAdded": [
			{"type": "like"}
		],
		"reactionsRemoved": [
			{"type": "heart"}
		]
	}`

	activity, err := schema.TeamsActivityFromJSONString(jsonStr)
	if err != nil {
		t.Fatalf("Failed to parse JSON: %v", err)
	}

	args := NewMessageReactionArgs(activity)

	if len(args.ReactionsAdded) != 1 {
		t.Fatalf("Expected 1 reaction added, got %d", len(args.ReactionsAdded))
	}
	if args.ReactionsAdded[0].Type != "like" {
		t.Errorf("Expected reaction type 'like', got '%s'", args.ReactionsAdded[0].Type)
	}

	if len(args.ReactionsRemoved) != 1 {
		t.Fatalf("Expected 1 reaction removed, got %d", len(args.ReactionsRemoved))
	}
	if args.ReactionsRemoved[0].Type != "heart" {
		t.Errorf("Expected reaction type 'heart', got '%s'", args.ReactionsRemoved[0].Type)
	}
}

func TestNewInstallationUpdateArgs(t *testing.T) {
	jsonStr := `{
		"type": "installationUpdate",
		"action": "add",
		"channelData": {
			"settings": {
				"selectedChannel": {
					"id": "channel-123"
				}
			}
		}
	}`

	activity, err := schema.TeamsActivityFromJSONString(jsonStr)
	if err != nil {
		t.Fatalf("Failed to parse JSON: %v", err)
	}

	args := NewInstallationUpdateArgs(activity)

	if args.Action != "add" {
		t.Errorf("Expected action 'add', got '%s'", args.Action)
	}
	if !args.IsAdd() {
		t.Error("Expected IsAdd() to be true")
	}
	if args.IsRemove() {
		t.Error("Expected IsRemove() to be false")
	}
	if args.SelectedChannelID != "channel-123" {
		t.Errorf("Expected selectedChannelId 'channel-123', got '%s'", args.SelectedChannelID)
	}
}

func TestNewInstallationUpdateArgsRemove(t *testing.T) {
	jsonStr := `{
		"type": "installationUpdate",
		"action": "remove"
	}`

	activity, err := schema.TeamsActivityFromJSONString(jsonStr)
	if err != nil {
		t.Fatalf("Failed to parse JSON: %v", err)
	}

	args := NewInstallationUpdateArgs(activity)

	if args.Action != "remove" {
		t.Errorf("Expected action 'remove', got '%s'", args.Action)
	}
	if args.IsAdd() {
		t.Error("Expected IsAdd() to be false")
	}
	if !args.IsRemove() {
		t.Error("Expected IsRemove() to be true")
	}
}

func TestNewConversationUpdateArgs(t *testing.T) {
	jsonStr := `{
		"type": "conversationUpdate",
		"membersAdded": [
			{"id": "user1", "name": "User One"},
			{"id": "user2", "name": "User Two"}
		],
		"membersRemoved": [
			{"id": "user3", "name": "User Three"}
		]
	}`

	activity, err := schema.TeamsActivityFromJSONString(jsonStr)
	if err != nil {
		t.Fatalf("Failed to parse JSON: %v", err)
	}

	args := NewConversationUpdateArgs(activity)

	if len(args.MembersAdded) != 2 {
		t.Fatalf("Expected 2 members added, got %d", len(args.MembersAdded))
	}
	if args.MembersAdded[0].ID != "user1" || args.MembersAdded[0].Name != "User One" {
		t.Error("Expected first member to be user1/User One")
	}
	if args.MembersAdded[1].ID != "user2" || args.MembersAdded[1].Name != "User Two" {
		t.Error("Expected second member to be user2/User Two")
	}

	if len(args.MembersRemoved) != 1 {
		t.Fatalf("Expected 1 member removed, got %d", len(args.MembersRemoved))
	}
	if args.MembersRemoved[0].ID != "user3" || args.MembersRemoved[0].Name != "User Three" {
		t.Error("Expected removed member to be user3/User Three")
	}
}

// mockBot is a mock implementation for testing Context
type mockBot struct {
	lastActivity interface{}
}

func (m *mockBot) SendActivity(ctx context.Context, activity interface{}) (string, error) {
	m.lastActivity = activity
	return "mock-id", nil
}

func TestContextSendActivity(t *testing.T) {
	activity := &schema.TeamsActivity{
		Type:       "message",
		ID:         "activity-1",
		ServiceURL: "https://example.com/",
		ChannelID:  "msteams",
		From:       &schema.TeamsConversationAccount{ID: "user-1", Name: "User"},
		Recipient:  &schema.TeamsConversationAccount{ID: "bot-1", Name: "Bot"},
		Conversation: &schema.TeamsConversation{ID: "conv-1"},
	}

	bot := &mockBot{}
	ctx := NewContext(activity, bot)

	id, err := ctx.SendActivity(context.Background(), "Hello!")
	if err != nil {
		t.Fatalf("SendActivity failed: %v", err)
	}
	if id != "mock-id" {
		t.Errorf("Expected id 'mock-id', got '%s'", id)
	}

	// Verify reply was created correctly
	if bot.lastActivity == nil {
		t.Fatal("Expected activity to be sent")
	}

	// Convert to JSON for inspection
	jsonBytes, _ := json.Marshal(bot.lastActivity)
	var reply map[string]interface{}
	json.Unmarshal(jsonBytes, &reply)

	if reply["text"] != "Hello!" {
		t.Errorf("Expected text 'Hello!', got '%v'", reply["text"])
	}
	if reply["type"] != "message" {
		t.Errorf("Expected type 'message', got '%v'", reply["type"])
	}
}
