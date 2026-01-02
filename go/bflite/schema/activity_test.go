package schema

import (
	"encoding/json"
	"testing"
)

func TestActivityCtorAndNulls(t *testing.T) {
	a1 := &Activity{Type: "message"}
	if a1 == nil {
		t.Error("Activity should not be nil")
	}
	if a1.Type != "message" {
		t.Errorf("Expected type 'message', got '%s'", a1.Type)
	}
	if a1.Text != "" {
		t.Error("Text should be empty")
	}

	a2 := &Activity{Type: "mytype"}
	if a2 == nil {
		t.Error("Activity should not be nil")
	}
	if a2.Type != "mytype" {
		t.Errorf("Expected type 'mytype', got '%s'", a2.Type)
	}
	if a2.Text != "" {
		t.Error("Text should be empty")
	}
}

func TestJSONNullsNotDeserialized(t *testing.T) {
	jsonStr := `{
		"type": "message",
		"text": null
	}`

	act, err := ActivityFromJSONString(jsonStr)
	if err != nil {
		t.Fatalf("Failed to parse JSON: %v", err)
	}
	if act.Type != "message" {
		t.Errorf("Expected type 'message', got '%s'", act.Type)
	}
	if act.Text != "" {
		t.Error("Text should be empty")
	}

	jsonStr2 := `{
		"type": "message"
	}`

	act2, err := ActivityFromJSONString(jsonStr2)
	if err != nil {
		t.Fatalf("Failed to parse JSON: %v", err)
	}
	if act2.Type != "message" {
		t.Errorf("Expected type 'message', got '%s'", act2.Type)
	}
	if act2.Text != "" {
		t.Error("Text should be empty")
	}
}

func TestAcceptUnknownPrimitiveFields(t *testing.T) {
	jsonStr := `{
		"type": "message",
		"text": "hello",
		"unknownString": "some string",
		"unknownInt": 123,
		"unknownBool": true,
		"unknownNull": null
	}`

	act, err := ActivityFromJSONString(jsonStr)
	if err != nil {
		t.Fatalf("Failed to parse JSON: %v", err)
	}
	if act.Type != "message" {
		t.Errorf("Expected type 'message', got '%s'", act.Type)
	}
	if act.Text != "hello" {
		t.Errorf("Expected text 'hello', got '%s'", act.Text)
	}

	// Check extension data
	if act.ExtensionData == nil {
		t.Fatal("ExtensionData should not be nil")
	}

	if _, ok := act.ExtensionData["unknownString"]; !ok {
		t.Error("Expected unknownString in ExtensionData")
	}
	if _, ok := act.ExtensionData["unknownInt"]; !ok {
		t.Error("Expected unknownInt in ExtensionData")
	}
	if _, ok := act.ExtensionData["unknownBool"]; !ok {
		t.Error("Expected unknownBool in ExtensionData")
	}
	if _, ok := act.ExtensionData["unknownNull"]; !ok {
		t.Error("Expected unknownNull in ExtensionData")
	}

	// Verify values
	var s string
	json.Unmarshal(act.ExtensionData["unknownString"], &s)
	if s != "some string" {
		t.Errorf("Expected 'some string', got '%s'", s)
	}

	var i int
	json.Unmarshal(act.ExtensionData["unknownInt"], &i)
	if i != 123 {
		t.Errorf("Expected 123, got %d", i)
	}

	var b bool
	json.Unmarshal(act.ExtensionData["unknownBool"], &b)
	if !b {
		t.Error("Expected true")
	}
}

func TestSerializeUnknownPrimitiveFields(t *testing.T) {
	act := &Activity{
		Type: "message",
		Text: "hello",
		ExtensionData: map[string]json.RawMessage{
			"unknownString": json.RawMessage(`"some string"`),
			"unknownInt":    json.RawMessage(`123`),
			"unknownBool":   json.RawMessage(`true`),
			"unknownNull":   json.RawMessage(`null`),
		},
	}

	jsonBytes, err := act.ToJSON()
	if err != nil {
		t.Fatalf("Failed to serialize: %v", err)
	}
	jsonStr := string(jsonBytes)

	// Check known fields are present
	if jsonStr == "" {
		t.Error("JSON should not be empty")
	}

	// Parse back to verify
	var parsed map[string]interface{}
	if err := json.Unmarshal(jsonBytes, &parsed); err != nil {
		t.Fatalf("Failed to parse serialized JSON: %v", err)
	}

	if parsed["type"] != "message" {
		t.Error("Expected type 'message'")
	}
	if parsed["text"] != "hello" {
		t.Error("Expected text 'hello'")
	}
	if parsed["unknownString"] != "some string" {
		t.Error("Expected unknownString 'some string'")
	}
	if parsed["unknownInt"].(float64) != 123 {
		t.Error("Expected unknownInt 123")
	}
	if parsed["unknownBool"] != true {
		t.Error("Expected unknownBool true")
	}
}

func TestDeserializeUnknownFieldsInKnownObjects(t *testing.T) {
	jsonStr := `{
		"type": "message",
		"text": "hello",
		"from": {
			"id": "1",
			"name": "tester",
			"aadObjectId": "123"
		}
	}`

	act, err := ActivityFromJSONString(jsonStr)
	if err != nil {
		t.Fatalf("Failed to parse JSON: %v", err)
	}
	if act.Type != "message" {
		t.Errorf("Expected type 'message', got '%s'", act.Type)
	}
	if act.Text != "hello" {
		t.Errorf("Expected text 'hello', got '%s'", act.Text)
	}
	if act.From == nil {
		t.Fatal("From should not be nil")
	}
	if act.From.ID != "1" {
		t.Errorf("Expected From.ID '1', got '%s'", act.From.ID)
	}
	if act.From.Name != "tester" {
		t.Errorf("Expected From.Name 'tester', got '%s'", act.From.Name)
	}
	if act.From.AadObjectID() != "123" {
		t.Errorf("Expected From.AadObjectID '123', got '%s'", act.From.AadObjectID())
	}
}

func TestDeserializeSerializeUnknownFieldsInKnownObjects(t *testing.T) {
	jsonStr := `{
		"type": "message",
		"text": "hello",
		"from": {
			"id": "1",
			"name": "tester",
			"aadObjectId": "123"
		}
	}`

	act, err := ActivityFromJSONString(jsonStr)
	if err != nil {
		t.Fatalf("Failed to parse JSON: %v", err)
	}
	act.Text = "updated"

	jsonBytes, err := act.ToJSON()
	if err != nil {
		t.Fatalf("Failed to serialize: %v", err)
	}

	// Parse back to verify
	var parsed map[string]interface{}
	if err := json.Unmarshal(jsonBytes, &parsed); err != nil {
		t.Fatalf("Failed to parse serialized JSON: %v", err)
	}

	if parsed["type"] != "message" {
		t.Error("Expected type 'message'")
	}
	if parsed["text"] != "updated" {
		t.Error("Expected text 'updated'")
	}
	from := parsed["from"].(map[string]interface{})
	if from["id"] != "1" {
		t.Error("Expected from.id '1'")
	}
	if from["name"] != "tester" {
		t.Error("Expected from.name 'tester'")
	}
	if from["aadObjectId"] != "123" {
		t.Error("Expected from.aadObjectId '123'")
	}
}

func TestCreateReply(t *testing.T) {
	act := &Activity{
		Text:      "hello",
		ID:        "activity1",
		ChannelID: "channel1",
		ServiceURL: "http://service.url",
		From: &ConversationAccount{
			ID:   "user1",
			Name: "User One",
		},
		Recipient: &ConversationAccount{
			ID:   "bot1",
			Name: "Bot One",
		},
		Conversation: &Conversation{
			ID: "conversation1",
		},
	}

	reply := act.CreateReplyActivity("reply")
	if reply == nil {
		t.Fatal("Reply should not be nil")
	}
	if reply.Type != "message" {
		t.Errorf("Expected type 'message', got '%s'", reply.Type)
	}
	if reply.Text != "reply" {
		t.Errorf("Expected text 'reply', got '%s'", reply.Text)
	}
	if reply.ChannelID != "channel1" {
		t.Errorf("Expected channelId 'channel1', got '%s'", reply.ChannelID)
	}
	if reply.ServiceURL != "http://service.url" {
		t.Errorf("Expected serviceUrl 'http://service.url', got '%s'", reply.ServiceURL)
	}
	if reply.Conversation == nil || reply.Conversation.ID != "conversation1" {
		t.Error("Expected conversation.id 'conversation1'")
	}
	if reply.From == nil || reply.From.ID != "bot1" || reply.From.Name != "Bot One" {
		t.Error("Expected from to be swapped (bot1, Bot One)")
	}
	if reply.Recipient == nil || reply.Recipient.ID != "user1" || reply.Recipient.Name != "User One" {
		t.Error("Expected recipient to be swapped (user1, User One)")
	}
	if reply.ReplyToID != "activity1" {
		t.Errorf("Expected replyToId 'activity1', got '%s'", reply.ReplyToID)
	}
}
