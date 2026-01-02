package schema

import (
	"encoding/json"

	coreschema "github.com/agnte/Rido.BFLite/go/bflite/schema"
)

// TeamsConversation extends Conversation with Teams-specific fields.
type TeamsConversation struct {
	ID               string `json:"id,omitempty"`
	TenantID         string `json:"tenantId,omitempty"`
	ConversationType string `json:"conversationType,omitempty"`
	IsGroup          bool   `json:"isGroup,omitempty"`

	// ExtensionData stores unknown properties
	ExtensionData map[string]json.RawMessage `json:"-"`
}

// TeamsConversationFromCore creates a TeamsConversation from a core Conversation.
func TeamsConversationFromCore(c *coreschema.Conversation) *TeamsConversation {
	tc := &TeamsConversation{
		ID: c.ID,
	}

	if c.ExtensionData != nil {
		if tenantID, ok := c.ExtensionData["tenantId"]; ok {
			var t string
			if json.Unmarshal(tenantID, &t) == nil {
				tc.TenantID = t
			}
		}
		if convType, ok := c.ExtensionData["conversationType"]; ok {
			var ct string
			if json.Unmarshal(convType, &ct) == nil {
				tc.ConversationType = ct
			}
		}
		if isGroup, ok := c.ExtensionData["isGroup"]; ok {
			var ig bool
			if json.Unmarshal(isGroup, &ig) == nil {
				tc.IsGroup = ig
			}
		}
	}

	return tc
}

// MarshalJSON implements custom JSON marshaling.
func (tc *TeamsConversation) MarshalJSON() ([]byte, error) {
	type Alias TeamsConversation
	data, err := json.Marshal((*Alias)(tc))
	if err != nil {
		return nil, err
	}

	if len(tc.ExtensionData) == 0 {
		return data, nil
	}

	var m map[string]json.RawMessage
	if err := json.Unmarshal(data, &m); err != nil {
		return nil, err
	}
	for k, v := range tc.ExtensionData {
		if _, exists := m[k]; !exists {
			m[k] = v
		}
	}
	return json.Marshal(m)
}

// UnmarshalJSON implements custom JSON unmarshaling.
func (tc *TeamsConversation) UnmarshalJSON(data []byte) error {
	type Alias TeamsConversation
	aux := &struct {
		*Alias
	}{
		Alias: (*Alias)(tc),
	}

	if err := json.Unmarshal(data, aux); err != nil {
		return err
	}

	var raw map[string]json.RawMessage
	if err := json.Unmarshal(data, &raw); err != nil {
		return err
	}

	knownFields := []string{"id", "tenantId", "conversationType", "isGroup"}
	for _, field := range knownFields {
		delete(raw, field)
	}

	if len(raw) > 0 {
		tc.ExtensionData = raw
	}

	return nil
}
