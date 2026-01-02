package schema

import "encoding/json"

// ConversationAccount represents a user or bot in a conversation.
type ConversationAccount struct {
	ID   string `json:"id,omitempty"`
	Name string `json:"name,omitempty"`

	// ExtensionData stores unknown properties like aadObjectId, role, etc.
	ExtensionData map[string]json.RawMessage `json:"-"`
}

// MarshalJSON implements custom JSON marshaling to handle extension data.
func (ca *ConversationAccount) MarshalJSON() ([]byte, error) {
	type Alias ConversationAccount
	data, err := json.Marshal((*Alias)(ca))
	if err != nil {
		return nil, err
	}

	if len(ca.ExtensionData) == 0 {
		return data, nil
	}

	// Merge extension data with known fields
	var m map[string]json.RawMessage
	if err := json.Unmarshal(data, &m); err != nil {
		return nil, err
	}
	for k, v := range ca.ExtensionData {
		if _, exists := m[k]; !exists {
			m[k] = v
		}
	}
	return json.Marshal(m)
}

// UnmarshalJSON implements custom JSON unmarshaling to capture extension data.
func (ca *ConversationAccount) UnmarshalJSON(data []byte) error {
	type Alias ConversationAccount
	aux := &struct {
		*Alias
	}{
		Alias: (*Alias)(ca),
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
	knownFields := []string{"id", "name"}
	for _, field := range knownFields {
		delete(raw, field)
	}

	if len(raw) > 0 {
		ca.ExtensionData = raw
	}

	return nil
}

// GetProperty retrieves a property from ExtensionData as a string.
func (ca *ConversationAccount) GetProperty(key string) string {
	if ca.ExtensionData == nil {
		return ""
	}
	raw, ok := ca.ExtensionData[key]
	if !ok {
		return ""
	}
	var s string
	if err := json.Unmarshal(raw, &s); err != nil {
		return ""
	}
	return s
}

// AadObjectID returns the Azure AD Object ID if present.
func (ca *ConversationAccount) AadObjectID() string {
	return ca.GetProperty("aadObjectId")
}

// Role returns the role (user/bot) if present.
func (ca *ConversationAccount) Role() string {
	return ca.GetProperty("role")
}
