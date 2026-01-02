// Package schema provides core Bot Framework activity models.
package schema

import (
	"encoding/json"
	"io"
)

// Activity represents a Bot Framework activity - the core message/event model.
type Activity struct {
	Type         string               `json:"type,omitempty"`
	ID           string               `json:"id,omitempty"`
	ServiceURL   string               `json:"serviceUrl,omitempty"`
	ChannelID    string               `json:"channelId,omitempty"`
	Text         string               `json:"text,omitempty"`
	ReplyToID    string               `json:"replyToId,omitempty"`
	From         *ConversationAccount `json:"from,omitempty"`
	Recipient    *ConversationAccount `json:"recipient,omitempty"`
	Conversation *Conversation        `json:"conversation,omitempty"`
	ChannelData  json.RawMessage      `json:"channelData,omitempty"`
	Entities     json.RawMessage      `json:"entities,omitempty"`

	// ExtensionData stores unknown properties for round-trip serialization
	ExtensionData map[string]json.RawMessage `json:"-"`
}

// MarshalJSON implements custom JSON marshaling to handle extension data.
func (a *Activity) MarshalJSON() ([]byte, error) {
	type Alias Activity
	data, err := json.Marshal((*Alias)(a))
	if err != nil {
		return nil, err
	}

	if len(a.ExtensionData) == 0 {
		return data, nil
	}

	// Merge extension data with known fields
	var m map[string]json.RawMessage
	if err := json.Unmarshal(data, &m); err != nil {
		return nil, err
	}
	for k, v := range a.ExtensionData {
		if _, exists := m[k]; !exists {
			m[k] = v
		}
	}
	return json.Marshal(m)
}

// UnmarshalJSON implements custom JSON unmarshaling to capture extension data.
func (a *Activity) UnmarshalJSON(data []byte) error {
	type Alias Activity
	aux := &struct {
		*Alias
	}{
		Alias: (*Alias)(a),
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
		a.ExtensionData = raw
	}

	return nil
}

// FromJSONReader deserializes an Activity from a JSON reader.
func (a *Activity) FromJSONReader(r io.Reader) error {
	return json.NewDecoder(r).Decode(a)
}

// FromJSONString deserializes an Activity from a JSON string.
func ActivityFromJSONString(jsonStr string) (*Activity, error) {
	var a Activity
	if err := json.Unmarshal([]byte(jsonStr), &a); err != nil {
		return nil, err
	}
	return &a, nil
}

// ToJSON serializes the Activity to JSON bytes.
func (a *Activity) ToJSON() ([]byte, error) {
	return json.MarshalIndent(a, "", "  ")
}

// ToJSONString serializes the Activity to a JSON string.
func (a *Activity) ToJSONString() (string, error) {
	data, err := a.ToJSON()
	if err != nil {
		return "", err
	}
	return string(data), nil
}

// CreateReplyActivity creates a reply activity from this activity.
// FR-005: Creates reply activities that copies Conversation, ServiceUrl, ChannelId from original,
// swaps From and Recipient, and sets ReplyToId to original activity ID.
func (a *Activity) CreateReplyActivity(text string) *Activity {
	return &Activity{
		Type:         "message",
		ChannelID:    a.ChannelID,
		ServiceURL:   a.ServiceURL,
		Conversation: a.Conversation,
		From:         a.Recipient,
		Recipient:    a.From,
		ReplyToID:    a.ID,
		Text:         text,
	}
}

// NewMessageActivity creates a new message activity with the specified text.
func NewMessageActivity(text string) *Activity {
	return &Activity{
		Type: "message",
		Text: text,
	}
}
