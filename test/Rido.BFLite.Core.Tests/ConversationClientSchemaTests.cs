using Rido.BFLite.Core.Schema;
using System.Text.Json;

namespace Rido.BFLite.Core.Tests
{
    public class ConversationClientSchemaTests
    {
        [Fact]
        public void ResourceResponse_Serialize_Deserialize()
        {
            string json = """
            {
                "id": "activity123"
            }
            """;
            ResourceResponse? response = JsonSerializer.Deserialize<ResourceResponse>(json, Activity.DefaultJsonOptions);
            Assert.NotNull(response);
            Assert.Equal("activity123", response.Id);

            string serialized = JsonSerializer.Serialize(response, Activity.DefaultJsonOptions);
            Assert.Contains("\"id\": \"activity123\"", serialized);
        }

        [Fact]
        public void ConversationResourceResponse_Serialize_Deserialize()
        {
            string json = """
            {
                "id": "conv123",
                "activityId": "activity456",
                "serviceUrl": "https://smba.trafficmanager.net/teams/"
            }
            """;
            ConversationResourceResponse? response = JsonSerializer.Deserialize<ConversationResourceResponse>(json, Activity.DefaultJsonOptions);
            Assert.NotNull(response);
            Assert.Equal("conv123", response.Id);
            Assert.Equal("activity456", response.ActivityId);
            Assert.Equal("https://smba.trafficmanager.net/teams/", response.ServiceUrl);

            string serialized = JsonSerializer.Serialize(response, Activity.DefaultJsonOptions);
            Assert.Contains("\"id\": \"conv123\"", serialized);
            Assert.Contains("\"activityId\": \"activity456\"", serialized);
            Assert.Contains("\"serviceUrl\": \"https://smba.trafficmanager.net/teams/\"", serialized);
        }

        [Fact]
        public void PagedMembersResult_Serialize_Deserialize()
        {
            string json = """
            {
                "continuationToken": "token123",
                "members": [
                    {
                        "id": "user1",
                        "name": "User One"
                    },
                    {
                        "id": "user2",
                        "name": "User Two"
                    }
                ]
            }
            """;
            PagedMembersResult? result = JsonSerializer.Deserialize<PagedMembersResult>(json, Activity.DefaultJsonOptions);
            Assert.NotNull(result);
            Assert.Equal("token123", result.ContinuationToken);
            Assert.NotNull(result.Members);
            Assert.Equal(2, result.Members.Count);
            Assert.Equal("user1", result.Members[0].Id);
            Assert.Equal("User One", result.Members[0].Name);
            Assert.Equal("user2", result.Members[1].Id);
            Assert.Equal("User Two", result.Members[1].Name);

            string serialized = JsonSerializer.Serialize(result, Activity.DefaultJsonOptions);
            Assert.Contains("\"continuationToken\": \"token123\"", serialized);
            Assert.Contains("\"id\": \"user1\"", serialized);
        }

        [Fact]
        public void ConversationParameters_Serialize_Deserialize()
        {
            ConversationParameters parameters = new()
            {
                IsGroup = true,
                Bot = new ConversationAccount { Id = "bot1", Name = "My Bot" },
                Members = new List<ConversationAccount>
                {
                    new ConversationAccount { Id = "user1", Name = "User One" },
                    new ConversationAccount { Id = "user2", Name = "User Two" }
                },
                TopicName = "Test Conversation",
                TenantId = "tenant123"
            };

            string serialized = JsonSerializer.Serialize(parameters, Activity.DefaultJsonOptions);
            Assert.Contains("\"isGroup\": true", serialized);
            Assert.Contains("\"topicName\": \"Test Conversation\"", serialized);
            Assert.Contains("\"tenantId\": \"tenant123\"", serialized);

            ConversationParameters? deserialized = JsonSerializer.Deserialize<ConversationParameters>(serialized, Activity.DefaultJsonOptions);
            Assert.NotNull(deserialized);
            Assert.True(deserialized.IsGroup);
            Assert.Equal("Test Conversation", deserialized.TopicName);
            Assert.Equal("tenant123", deserialized.TenantId);
            Assert.NotNull(deserialized.Bot);
            Assert.Equal("bot1", deserialized.Bot.Id);
            Assert.NotNull(deserialized.Members);
            Assert.Equal(2, deserialized.Members.Count);
        }

        [Fact]
        public void ConversationParameters_With_Activity()
        {
            Activity activity = new()
            {
                Type = "message",
                Text = "Initial message"
            };

            ConversationParameters parameters = new()
            {
                Bot = new ConversationAccount { Id = "bot1" },
                Members = new List<ConversationAccount>
                {
                    new ConversationAccount { Id = "user1" }
                },
                Activity = activity
            };

            string serialized = JsonSerializer.Serialize(parameters, Activity.DefaultJsonOptions);
            Assert.Contains("\"activity\":", serialized);
            Assert.Contains("\"text\": \"Initial message\"", serialized);

            ConversationParameters? deserialized = JsonSerializer.Deserialize<ConversationParameters>(serialized, Activity.DefaultJsonOptions);
            Assert.NotNull(deserialized);
            Assert.NotNull(deserialized.Activity);
            Assert.Equal("message", deserialized.Activity.Type);
            Assert.Equal("Initial message", deserialized.Activity.Text);
        }

        [Fact]
        public void PagedMembersResult_Empty_Members()
        {
            PagedMembersResult result = new()
            {
                Members = new List<ConversationAccount>()
            };

            string serialized = JsonSerializer.Serialize(result, Activity.DefaultJsonOptions);
            Assert.Contains("\"members\": []", serialized);

            PagedMembersResult? deserialized = JsonSerializer.Deserialize<PagedMembersResult>(serialized, Activity.DefaultJsonOptions);
            Assert.NotNull(deserialized);
            Assert.NotNull(deserialized.Members);
            Assert.Empty(deserialized.Members);
            Assert.Null(deserialized.ContinuationToken);
        }

        [Fact]
        public void ConversationParameters_Extension_Properties()
        {
            string json = """
            {
                "bot": {
                    "id": "bot1"
                },
                "members": [
                    {
                        "id": "user1"
                    }
                ],
                "customProperty": "customValue"
            }
            """;

            ConversationParameters? parameters = JsonSerializer.Deserialize<ConversationParameters>(json, Activity.DefaultJsonOptions);
            Assert.NotNull(parameters);
            Assert.NotNull(parameters.Bot);
            Assert.Equal("bot1", parameters.Bot.Id);
            Assert.True(parameters.Properties.ContainsKey("customProperty"));
            Assert.Equal("customValue", parameters.Properties["customProperty"]?.ToString());
        }
    }
}
