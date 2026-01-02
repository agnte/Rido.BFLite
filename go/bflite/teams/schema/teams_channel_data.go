package schema

import "encoding/json"

// TeamsChannelData contains Teams-specific channel data.
type TeamsChannelData struct {
	ClientActivityID string                    `json:"clientActivityID,omitempty"`
	TeamsChannelID   string                    `json:"teamsChannelId,omitempty"`
	TeamsTeamID      string                    `json:"teamsTeamId,omitempty"`
	Channel          *TeamsChannel             `json:"channel,omitempty"`
	Team             *Team                     `json:"team,omitempty"`
	Tenant           *TeamsChannelDataTenant   `json:"tenant,omitempty"`
	Settings         *TeamsChannelDataSettings `json:"settings,omitempty"`

	// ExtensionData stores unknown properties
	ExtensionData map[string]json.RawMessage `json:"-"`
}

// TeamsChannel represents a Teams channel.
type TeamsChannel struct {
	ID   string `json:"id,omitempty"`
	Name string `json:"name,omitempty"`
}

// Team represents a Teams team.
type Team struct {
	ID           string `json:"id,omitempty"`
	Name         string `json:"name,omitempty"`
	AadGroupID   string `json:"aadGroupId,omitempty"`
	TenantID     string `json:"tenantId,omitempty"`
	Type         string `json:"type,omitempty"`
	ChannelCount int    `json:"channelCount,omitempty"`
	MemberCount  int    `json:"memberCount,omitempty"`
}

// TeamsChannelDataTenant represents tenant information.
type TeamsChannelDataTenant struct {
	ID string `json:"id,omitempty"`
}

// TeamsChannelDataSettings contains settings data.
type TeamsChannelDataSettings struct {
	SelectedChannel *TeamsChannel `json:"selectedChannel,omitempty"`
}

// MarshalJSON implements custom JSON marshaling.
func (tcd *TeamsChannelData) MarshalJSON() ([]byte, error) {
	type Alias TeamsChannelData
	data, err := json.Marshal((*Alias)(tcd))
	if err != nil {
		return nil, err
	}

	if len(tcd.ExtensionData) == 0 {
		return data, nil
	}

	var m map[string]json.RawMessage
	if err := json.Unmarshal(data, &m); err != nil {
		return nil, err
	}
	for k, v := range tcd.ExtensionData {
		if _, exists := m[k]; !exists {
			m[k] = v
		}
	}
	return json.Marshal(m)
}

// UnmarshalJSON implements custom JSON unmarshaling.
func (tcd *TeamsChannelData) UnmarshalJSON(data []byte) error {
	type Alias TeamsChannelData
	aux := &struct {
		*Alias
	}{
		Alias: (*Alias)(tcd),
	}

	if err := json.Unmarshal(data, aux); err != nil {
		return err
	}

	var raw map[string]json.RawMessage
	if err := json.Unmarshal(data, &raw); err != nil {
		return err
	}

	knownFields := []string{
		"clientActivityID", "teamsChannelId", "teamsTeamId",
		"channel", "team", "tenant", "settings",
	}
	for _, field := range knownFields {
		delete(raw, field)
	}

	if len(raw) > 0 {
		tcd.ExtensionData = raw
	}

	return nil
}
