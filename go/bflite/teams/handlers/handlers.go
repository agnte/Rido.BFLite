// Package handlers provides handler types for Teams bot events.
package handlers

import (
	"context"
	"encoding/json"

	"github.com/agnte/Rido.BFLite/go/bflite/teams/schema"
)

// Context provides access to the current activity and bot application.
type Context struct {
	Activity       *schema.TeamsActivity
	botApplication interface {
		SendActivity(ctx context.Context, activity interface{}) (string, error)
	}
}

// NewContext creates a new handler context.
func NewContext(activity *schema.TeamsActivity, bot interface {
	SendActivity(ctx context.Context, activity interface{}) (string, error)
}) *Context {
	return &Context{
		Activity:       activity,
		botApplication: bot,
	}
}

// SendActivity sends a text message reply to the current conversation.
func (c *Context) SendActivity(ctx context.Context, text string) (string, error) {
	reply := c.Activity.CreateReplyActivity(text)
	return c.botApplication.SendActivity(ctx, reply)
}

// MessageHandler handles incoming message activities.
type MessageHandler func(ctx context.Context, c *Context) error

// MessageReactionArgs contains arguments for message reaction events.
type MessageReactionArgs struct {
	Activity         *schema.TeamsActivity
	ReactionsAdded   []MessageReaction
	ReactionsRemoved []MessageReaction
}

// MessageReaction represents a reaction on a message.
type MessageReaction struct {
	Type string `json:"type,omitempty"`
}

// NewMessageReactionArgs creates MessageReactionArgs from a TeamsActivity.
func NewMessageReactionArgs(activity *schema.TeamsActivity) *MessageReactionArgs {
	args := &MessageReactionArgs{
		Activity: activity,
	}

	// Parse reactionsAdded
	if raw := activity.GetExtensionProperty("reactionsAdded"); raw != nil {
		var reactions []MessageReaction
		if json.Unmarshal(raw, &reactions) == nil {
			args.ReactionsAdded = reactions
		}
	}

	// Parse reactionsRemoved
	if raw := activity.GetExtensionProperty("reactionsRemoved"); raw != nil {
		var reactions []MessageReaction
		if json.Unmarshal(raw, &reactions) == nil {
			args.ReactionsRemoved = reactions
		}
	}

	return args
}

// MessageReactionHandler handles message reaction activities.
type MessageReactionHandler func(ctx context.Context, args *MessageReactionArgs, c *Context) error

// InstallationUpdateArgs contains arguments for installation update events.
type InstallationUpdateArgs struct {
	Activity          *schema.TeamsActivity
	Action            string
	SelectedChannelID string
}

// IsAdd returns true if this is an add action.
func (a *InstallationUpdateArgs) IsAdd() bool {
	return a.Action == "add"
}

// IsRemove returns true if this is a remove action.
func (a *InstallationUpdateArgs) IsRemove() bool {
	return a.Action == "remove"
}

// NewInstallationUpdateArgs creates InstallationUpdateArgs from a TeamsActivity.
func NewInstallationUpdateArgs(activity *schema.TeamsActivity) *InstallationUpdateArgs {
	args := &InstallationUpdateArgs{
		Activity: activity,
		Action:   activity.GetExtensionPropertyString("action"),
	}

	// Get selected channel ID from channelData.settings.selectedChannel.id
	if activity.ChannelData != nil && activity.ChannelData.Settings != nil && activity.ChannelData.Settings.SelectedChannel != nil {
		args.SelectedChannelID = activity.ChannelData.Settings.SelectedChannel.ID
	}

	return args
}

// InstallationUpdateHandler handles installation update activities.
type InstallationUpdateHandler func(ctx context.Context, args *InstallationUpdateArgs, c *Context) error

// ConversationUpdateArgs contains arguments for conversation update events.
type ConversationUpdateArgs struct {
	Activity       *schema.TeamsActivity
	MembersAdded   []ConversationMember
	MembersRemoved []ConversationMember
}

// ConversationMember represents a member in a conversation update.
type ConversationMember struct {
	ID   string `json:"id,omitempty"`
	Name string `json:"name,omitempty"`
}

// NewConversationUpdateArgs creates ConversationUpdateArgs from a TeamsActivity.
func NewConversationUpdateArgs(activity *schema.TeamsActivity) *ConversationUpdateArgs {
	args := &ConversationUpdateArgs{
		Activity: activity,
	}

	// Parse membersAdded
	if raw := activity.GetExtensionProperty("membersAdded"); raw != nil {
		var members []ConversationMember
		if json.Unmarshal(raw, &members) == nil {
			args.MembersAdded = members
		}
	}

	// Parse membersRemoved
	if raw := activity.GetExtensionProperty("membersRemoved"); raw != nil {
		var members []ConversationMember
		if json.Unmarshal(raw, &members) == nil {
			args.MembersRemoved = members
		}
	}

	return args
}

// ConversationUpdateHandler handles conversation update activities.
type ConversationUpdateHandler func(ctx context.Context, args *ConversationUpdateArgs, c *Context) error
