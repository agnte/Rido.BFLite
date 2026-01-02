package schema

import "encoding/json"

// Conversation represents the conversation context.
type Conversation struct {
	ID string `json:"id,omitempty"`

	// ExtensionData stores unknown properties
	ExtensionData map[string]json.RawMessage `json:"-"`
}

// MarshalJSON implements custom JSON marshaling to handle extension data.
func (c *Conversation) MarshalJSON() ([]byte, error) {
	type Alias Conversation
	data, err := json.Marshal((*Alias)(c))
	if err != nil {
		return nil, err
	}

	if len(c.ExtensionData) == 0 {
		return data, nil
	}

	// Merge extension data with known fields
	var m map[string]json.RawMessage
	if err := json.Unmarshal(data, &m); err != nil {
		return nil, err
	}
	for k, v := range c.ExtensionData {
		if _, exists := m[k]; !exists {
			m[k] = v
		}
	}
	return json.Marshal(m)
}

// UnmarshalJSON implements custom JSON unmarshaling to capture extension data.
func (c *Conversation) UnmarshalJSON(data []byte) error {
	type Alias Conversation
	aux := &struct {
		*Alias
	}{
		Alias: (*Alias)(c),
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
	delete(raw, "id")

	if len(raw) > 0 {
		c.ExtensionData = raw
	}

	return nil
}

// GetProperty retrieves a property from ExtensionData as a string.
func (c *Conversation) GetProperty(key string) string {
	if c.ExtensionData == nil {
		return ""
	}
	raw, ok := c.ExtensionData[key]
	if !ok {
		return ""
	}
	var s string
	if err := json.Unmarshal(raw, &s); err != nil {
		return ""
	}
	return s
}
