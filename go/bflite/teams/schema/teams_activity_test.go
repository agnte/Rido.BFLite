package schema

import (
	"encoding/json"
	"testing"

	coreschema "github.com/agnte/Rido.BFLite/go/bflite/schema"
)

const teamsActivityJSON = `{
  "type": "message",
  "channelId": "msteams",
  "text": "<at>ridotest</at> reply to thread",
  "id": "1759944781430",
  "serviceUrl": "https://smba.trafficmanager.net/amer/50612dbb-0237-4969-b378-8d42590f9c00/",
  "channelData": {
    "teamsChannelId": "19:6848757105754c8981c67612732d9aa7@thread.tacv2",
    "teamsTeamId": "19:66P469zibfbsGI-_a0aN_toLTZpyzS6u7CT3TsXdgPw1@thread.tacv2",
    "channel": {
      "id": "19:6848757105754c8981c67612732d9aa7@thread.tacv2"
    },
    "team": {
      "id": "19:66P469zibfbsGI-_a0aN_toLTZpyzS6u7CT3TsXdgPw1@thread.tacv2"
    },
    "tenant": {
      "id": "50612dbb-0237-4969-b378-8d42590f9c00"
    }
  },
  "from": {
    "id": "29:17bUvCasIPKfQIXHvNzcPjD86fwm6GkWc1PvCGP2-NSkNb7AyGYpjQ7Xw-XgTwaHW5JxZ4KMNDxn1kcL8fwX1Nw",
    "name": "rido",
    "aadObjectId": "b15a9416-0ad3-4172-9210-7beb711d3f70"
  },
  "recipient": {
    "id": "28:0b6fe6d1-fece-44f7-9a48-56465e2d5ab8",
    "name": "ridotest"
  },
  "conversation": {
    "id": "19:6848757105754c8981c67612732d9aa7@thread.tacv2;messageid=1759881511856",
    "isGroup": true,
    "conversationType": "channel",
    "tenantId": "50612dbb-0237-4969-b378-8d42590f9c00"
  }
}`

func TestDeserializeTeamsActivity(t *testing.T) {
	activity, err := TeamsActivityFromJSONString(teamsActivityJSON)
	if err != nil {
		t.Fatalf("Failed to parse JSON: %v", err)
	}

	if activity.Type != "message" {
		t.Errorf("Expected type 'message', got '%s'", activity.Type)
	}
	if activity.ChannelID != "msteams" {
		t.Errorf("Expected channelId 'msteams', got '%s'", activity.ChannelID)
	}
	if activity.ChannelData == nil {
		t.Fatal("ChannelData should not be nil")
	}
	if activity.ChannelData.TeamsChannelID != "19:6848757105754c8981c67612732d9aa7@thread.tacv2" {
		t.Errorf("Expected teamsChannelId, got '%s'", activity.ChannelData.TeamsChannelID)
	}
	if activity.ChannelData.Channel == nil || activity.ChannelData.Channel.ID != "19:6848757105754c8981c67612732d9aa7@thread.tacv2" {
		t.Error("Expected channel.id")
	}
	if activity.From == nil || activity.From.AadObjectId != "b15a9416-0ad3-4172-9210-7beb711d3f70" {
		t.Error("Expected from.aadObjectId")
	}
	if activity.Conversation == nil || activity.Conversation.ID != "19:6848757105754c8981c67612732d9aa7@thread.tacv2;messageid=1759881511856" {
		t.Error("Expected conversation.id")
	}
}

func TestDowncastTeamsActivityToCoreActivity(t *testing.T) {
	// First parse as core Activity
	var activity coreschema.Activity
	if err := json.Unmarshal([]byte(teamsActivityJSON), &activity); err != nil {
		t.Fatalf("Failed to parse core Activity: %v", err)
	}

	if activity.Conversation == nil || activity.Conversation.ID != "19:6848757105754c8981c67612732d9aa7@thread.tacv2;messageid=1759881511856" {
		t.Error("Expected conversation.id in core activity")
	}

	// Convert to TeamsActivity
	teamsActivity := FromActivity(&activity)

	if teamsActivity.Conversation == nil || teamsActivity.Conversation.ID != "19:6848757105754c8981c67612732d9aa7@thread.tacv2;messageid=1759881511856" {
		t.Error("Expected conversation.id in teams activity")
	}

	// Verify Teams-specific fields were populated
	if teamsActivity.ChannelData == nil {
		t.Error("ChannelData should not be nil")
	}
	if teamsActivity.ChannelData.TeamsChannelID != "19:6848757105754c8981c67612732d9aa7@thread.tacv2" {
		t.Errorf("Expected teamsChannelId, got '%s'", teamsActivity.ChannelData.TeamsChannelID)
	}
}

func TestTeamsConversation(t *testing.T) {
	activity, err := TeamsActivityFromJSONString(teamsActivityJSON)
	if err != nil {
		t.Fatalf("Failed to parse JSON: %v", err)
	}

	if activity.Conversation == nil {
		t.Fatal("Conversation should not be nil")
	}
	if activity.Conversation.TenantID != "50612dbb-0237-4969-b378-8d42590f9c00" {
		t.Errorf("Expected tenantId, got '%s'", activity.Conversation.TenantID)
	}
	if activity.Conversation.ConversationType != "channel" {
		t.Errorf("Expected conversationType 'channel', got '%s'", activity.Conversation.ConversationType)
	}
	if !activity.Conversation.IsGroup {
		t.Error("Expected isGroup to be true")
	}
}

func TestTeamsChannelData(t *testing.T) {
	activity, err := TeamsActivityFromJSONString(teamsActivityJSON)
	if err != nil {
		t.Fatalf("Failed to parse JSON: %v", err)
	}

	if activity.ChannelData == nil {
		t.Fatal("ChannelData should not be nil")
	}
	if activity.ChannelData.Team == nil {
		t.Fatal("Team should not be nil")
	}
	if activity.ChannelData.Team.ID != "19:66P469zibfbsGI-_a0aN_toLTZpyzS6u7CT3TsXdgPw1@thread.tacv2" {
		t.Errorf("Expected team.id, got '%s'", activity.ChannelData.Team.ID)
	}
	if activity.ChannelData.Tenant == nil {
		t.Fatal("Tenant should not be nil")
	}
	if activity.ChannelData.Tenant.ID != "50612dbb-0237-4969-b378-8d42590f9c00" {
		t.Errorf("Expected tenant.id, got '%s'", activity.ChannelData.Tenant.ID)
	}
}
