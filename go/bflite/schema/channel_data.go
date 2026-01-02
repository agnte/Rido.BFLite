package schema

import "encoding/json"

// ChannelData represents base channel-specific data.
type ChannelData struct {
	ClientActivityID string `json:"clientActivityID,omitempty"`

	// ExtensionData stores unknown properties
	ExtensionData map[string]json.RawMessage `json:"-"`
}

// MarshalJSON implements custom JSON marshaling to handle extension data.
func (cd *ChannelData) MarshalJSON() ([]byte, error) {
	type Alias ChannelData
	data, err := json.Marshal((*Alias)(cd))
	if err != nil {
		return nil, err
	}

	if len(cd.ExtensionData) == 0 {
		return data, nil
	}

	// Merge extension data with known fields
	var m map[string]json.RawMessage
	if err := json.Unmarshal(data, &m); err != nil {
		return nil, err
	}
	for k, v := range cd.ExtensionData {
		if _, exists := m[k]; !exists {
			m[k] = v
		}
	}
	return json.Marshal(m)
}

// UnmarshalJSON implements custom JSON unmarshaling to capture extension data.
func (cd *ChannelData) UnmarshalJSON(data []byte) error {
	type Alias ChannelData
	aux := &struct {
		*Alias
	}{
		Alias: (*Alias)(cd),
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
	delete(raw, "clientActivityID")

	if len(raw) > 0 {
		cd.ExtensionData = raw
	}

	return nil
}
