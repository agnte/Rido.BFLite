package schema

import (
	"encoding/json"

	coreschema "github.com/agnte/Rido.BFLite/go/bflite/schema"
)

// TeamsConversationAccount extends ConversationAccount with Teams-specific fields.
type TeamsConversationAccount struct {
	ID          string `json:"id,omitempty"`
	Name        string `json:"name,omitempty"`
	AadObjectId string `json:"aadObjectId,omitempty"`
	Email       string `json:"email,omitempty"`
	UPN         string `json:"userPrincipalName,omitempty"`

	// ExtensionData stores unknown properties
	ExtensionData map[string]json.RawMessage `json:"-"`
}

// TeamsConversationAccountFromCore creates a TeamsConversationAccount from a core ConversationAccount.
func TeamsConversationAccountFromCore(ca *coreschema.ConversationAccount) *TeamsConversationAccount {
	tca := &TeamsConversationAccount{
		ID:          ca.ID,
		Name:        ca.Name,
		AadObjectId: ca.AadObjectID(),
	}

	// Check for additional Teams-specific properties
	if ca.ExtensionData != nil {
		if email, ok := ca.ExtensionData["email"]; ok {
			var e string
			if json.Unmarshal(email, &e) == nil {
				tca.Email = e
			}
		}
		if upn, ok := ca.ExtensionData["userPrincipalName"]; ok {
			var u string
			if json.Unmarshal(upn, &u) == nil {
				tca.UPN = u
			}
		}
	}

	return tca
}

// MarshalJSON implements custom JSON marshaling.
func (tca *TeamsConversationAccount) MarshalJSON() ([]byte, error) {
	type Alias TeamsConversationAccount
	data, err := json.Marshal((*Alias)(tca))
	if err != nil {
		return nil, err
	}

	if len(tca.ExtensionData) == 0 {
		return data, nil
	}

	var m map[string]json.RawMessage
	if err := json.Unmarshal(data, &m); err != nil {
		return nil, err
	}
	for k, v := range tca.ExtensionData {
		if _, exists := m[k]; !exists {
			m[k] = v
		}
	}
	return json.Marshal(m)
}

// UnmarshalJSON implements custom JSON unmarshaling.
func (tca *TeamsConversationAccount) UnmarshalJSON(data []byte) error {
	type Alias TeamsConversationAccount
	aux := &struct {
		*Alias
	}{
		Alias: (*Alias)(tca),
	}

	if err := json.Unmarshal(data, aux); err != nil {
		return err
	}

	var raw map[string]json.RawMessage
	if err := json.Unmarshal(data, &raw); err != nil {
		return err
	}

	knownFields := []string{"id", "name", "aadObjectId", "email", "userPrincipalName"}
	for _, field := range knownFields {
		delete(raw, field)
	}

	if len(raw) > 0 {
		tca.ExtensionData = raw
	}

	return nil
}
