// Package schema provides Teams-specific Bot Framework activity models.
package schema

import (
	"encoding/json"

	coreschema "github.com/agnte/Rido.BFLite/go/bflite/schema"
)

// ActivityTypes contains Teams activity type constants.
var ActivityTypes = struct {
	Message            string
	ConversationUpdate string
	InstallationUpdate string
	MessageReaction    string
}{
	Message:            "message",
	ConversationUpdate: "conversationUpdate",
	InstallationUpdate: "installationUpdate",
	MessageReaction:    "messageReaction",
}

// TeamsActivity extends the core Activity with Teams-specific fields.
type TeamsActivity struct {
	Type         string                    `json:"type,omitempty"`
	ID           string                    `json:"id,omitempty"`
	ServiceURL   string                    `json:"serviceUrl,omitempty"`
	ChannelID    string                    `json:"channelId,omitempty"`
	Text         string                    `json:"text,omitempty"`
	ReplyToID    string                    `json:"replyToId,omitempty"`
	From         *TeamsConversationAccount `json:"from,omitempty"`
	Recipient    *TeamsConversationAccount `json:"recipient,omitempty"`
	Conversation *TeamsConversation        `json:"conversation,omitempty"`
	ChannelData  *TeamsChannelData         `json:"channelData,omitempty"`
	Entities     json.RawMessage           `json:"entities,omitempty"`

	// ExtensionData stores unknown properties for round-trip serialization
	ExtensionData map[string]json.RawMessage `json:"-"`
}

// FromActivity creates a TeamsActivity from a core Activity.
func FromActivity(activity *coreschema.Activity) *TeamsActivity {
	ta := &TeamsActivity{
		Type:          activity.Type,
		ID:            activity.ID,
		ServiceURL:    activity.ServiceURL,
		ChannelID:     activity.ChannelID,
		Text:          activity.Text,
		ReplyToID:     activity.ReplyToID,
		Entities:      activity.Entities,
		ExtensionData: activity.ExtensionData,
	}

	// Convert From to TeamsConversationAccount
	if activity.From != nil {
		ta.From = TeamsConversationAccountFromCore(activity.From)
	}

	// Convert Recipient to TeamsConversationAccount
	if activity.Recipient != nil {
		ta.Recipient = TeamsConversationAccountFromCore(activity.Recipient)
	}

	// Convert Conversation to TeamsConversation
	if activity.Conversation != nil {
		ta.Conversation = TeamsConversationFromCore(activity.Conversation)
	}

	// Parse ChannelData into TeamsChannelData
	if len(activity.ChannelData) > 0 {
		var tcd TeamsChannelData
		if err := json.Unmarshal(activity.ChannelData, &tcd); err == nil {
			ta.ChannelData = &tcd
		}
	}

	return ta
}

// CreateReplyActivity creates a reply activity from this Teams activity.
func (ta *TeamsActivity) CreateReplyActivity(text string) *coreschema.Activity {
	reply := &coreschema.Activity{
		Type:       "message",
		ChannelID:  ta.ChannelID,
		ServiceURL: ta.ServiceURL,
		ReplyToID:  ta.ID,
		Text:       text,
	}

	if ta.Conversation != nil {
		reply.Conversation = &coreschema.Conversation{ID: ta.Conversation.ID}
	}
	if ta.Recipient != nil {
		reply.From = &coreschema.ConversationAccount{ID: ta.Recipient.ID, Name: ta.Recipient.Name}
	}
	if ta.From != nil {
		reply.Recipient = &coreschema.ConversationAccount{ID: ta.From.ID, Name: ta.From.Name}
	}

	return reply
}

// MarshalJSON implements custom JSON marshaling to handle extension data.
func (ta *TeamsActivity) MarshalJSON() ([]byte, error) {
	type Alias TeamsActivity
	data, err := json.Marshal((*Alias)(ta))
	if err != nil {
		return nil, err
	}

	if len(ta.ExtensionData) == 0 {
		return data, nil
	}

	// Merge extension data with known fields
	var m map[string]json.RawMessage
	if err := json.Unmarshal(data, &m); err != nil {
		return nil, err
	}
	for k, v := range ta.ExtensionData {
		if _, exists := m[k]; !exists {
			m[k] = v
		}
	}
	return json.Marshal(m)
}

// UnmarshalJSON implements custom JSON unmarshaling to capture extension data.
func (ta *TeamsActivity) UnmarshalJSON(data []byte) error {
	type Alias TeamsActivity
	aux := &struct {
		*Alias
	}{
		Alias: (*Alias)(ta),
	}

	if err := json.Unmarshal(data, aux); err != nil {
		return err
	}

	// Capture all fields into a map
	var raw map[string]json.RawMessage
	if err := json.Unmarshal(data, &raw); err != nil {
		return err
	}

	// Remove known fields from extension data
	knownFields := []string{
		"type", "id", "serviceUrl", "channelId", "text", "replyToId",
		"from", "recipient", "conversation", "channelData", "entities",
	}
	for _, field := range knownFields {
		delete(raw, field)
	}

	if len(raw) > 0 {
		ta.ExtensionData = raw
	}

	return nil
}

// TeamsActivityFromJSONString parses a JSON string into a TeamsActivity.
func TeamsActivityFromJSONString(jsonStr string) (*TeamsActivity, error) {
	var ta TeamsActivity
	if err := json.Unmarshal([]byte(jsonStr), &ta); err != nil {
		return nil, err
	}
	return &ta, nil
}

// GetExtensionProperty retrieves a property from ExtensionData.
func (ta *TeamsActivity) GetExtensionProperty(key string) json.RawMessage {
	if ta.ExtensionData == nil {
		return nil
	}
	return ta.ExtensionData[key]
}

// GetExtensionPropertyString retrieves a string property from ExtensionData.
func (ta *TeamsActivity) GetExtensionPropertyString(key string) string {
	raw := ta.GetExtensionProperty(key)
	if raw == nil {
		return ""
	}
	var s string
	if err := json.Unmarshal(raw, &s); err != nil {
		return ""
	}
	return s
}
