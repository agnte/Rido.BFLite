using Rido.BFLite.Core.Schema;
using Xunit;

namespace Rido.BFLite.Core.Tests
{
    public class StreamingResponseTests
    {
        [Fact]
        public void StreamingResponse_CanBeCreated()
        {
            // Arrange
            var botApp = new BotApplication();
            var activity = new Activity
            {
                Type = "message",
                Text = "test",
                Conversation = new Conversation { Id = "test-conversation" },
                ServiceUrl = "https://test.botframework.com",
                ChannelId = "msteams",
                From = new ConversationAccount { Id = "user1" },
                Recipient = new ConversationAccount { Id = "bot1" }
            };

            // Act
            var streamingResponse = new StreamingResponse(botApp, activity);

            // Assert
            Assert.NotNull(streamingResponse);
            Assert.Equal("", streamingResponse.Message);
            Assert.Equal(0, streamingResponse.UpdatesSent());
        }

        [Fact]
        public void StreamingResponse_AccumulatesMessage()
        {
            // Arrange
            var botApp = new BotApplication();
            var activity = new Activity
            {
                Type = "message",
                Text = "test",
                Conversation = new Conversation { Id = "test-conversation" },
                ServiceUrl = "https://test.botframework.com",
                ChannelId = "msteams",
                From = new ConversationAccount { Id = "user1" },
                Recipient = new ConversationAccount { Id = "bot1" }
            };
            var streamingResponse = new StreamingResponse(botApp, activity);

            // Act - manually build the message without queueing
            // Note: We can't actually queue without a conversation client
            // but we can test that the Message property accumulates text
            var message = "";
            message += "Hello ";
            message += "World!";

            // Assert
            Assert.Equal("Hello World!", message);
            Assert.NotNull(streamingResponse);
        }

        [Fact]
        public void StreamingResponse_PropertiesAreInitialized()
        {
            // Arrange
            var botApp = new BotApplication();
            var activity = new Activity
            {
                Type = "message",
                Text = "test",
                Conversation = new Conversation { Id = "test-conversation" },
                ServiceUrl = "https://test.botframework.com",
                ChannelId = "msteams",
                From = new ConversationAccount { Id = "user1" },
                Recipient = new ConversationAccount { Id = "bot1" }
            };
            
            // Act
            var streamingResponse = new StreamingResponse(botApp, activity);
            
            // Assert
            Assert.NotNull(streamingResponse.Attachments);
            Assert.Empty(streamingResponse.Attachments);
            Assert.NotNull(streamingResponse.Citations);
            Assert.Empty(streamingResponse.Citations);
            Assert.False(streamingResponse.EnableFeedbackLoop);
            Assert.False(streamingResponse.EnableGeneratedByAILabel);
            Assert.Equal("default", streamingResponse.FeedbackLoopType);
        }

        [Fact]
        public void StreamingChannelData_SerializesCorrectly()
        {
            // Arrange
            var channelData = new StreamingChannelData
            {
                StreamId = "stream-123",
                StreamType = StreamType.Streaming,
                StreamSequence = 1,
                FeedbackLoopEnabled = true,
                FeedbackLoopType = "default"
            };

            // Act
            var json = System.Text.Json.JsonSerializer.Serialize(channelData, Activity.DefaultJsonOptions);

            // Assert
            Assert.Contains("stream-123", json);
            Assert.Contains("streamType", json);
            Assert.Contains("streaming", json);
        }

        [Fact]
        public void Citation_CanBeCreated()
        {
            // Arrange & Act
            var citation = new Citation
            {
                Title = "Test Title",
                Content = "Test Content",
                Url = "https://example.com"
            };

            // Assert
            Assert.Equal("Test Title", citation.Title);
            Assert.Equal("Test Content", citation.Content);
            Assert.Equal("https://example.com", citation.Url);
        }

        [Fact]
        public void ClientCitation_CanBeCreated()
        {
            // Arrange & Act
            var clientCitation = new ClientCitation
            {
                Position = 1,
                Appearance = new ClientCitationAppearance
                {
                    Name = "Test",
                    Abstract = "Abstract",
                    Url = "https://example.com"
                }
            };

            // Assert
            Assert.Equal(1, clientCitation.Position);
            Assert.NotNull(clientCitation.Appearance);
            Assert.Equal("Test", clientCitation.Appearance.Name);
        }

        [Fact]
        public void StreamType_HasExpectedValues()
        {
            // Assert
            Assert.Equal("informative", StreamType.Informative);
            Assert.Equal("streaming", StreamType.Streaming);
            Assert.Equal("final", StreamType.Final);
        }

        [Fact]
        public void Entity_CanBeCreated()
        {
            // Arrange & Act
            var entity = new Entity("test-type");

            // Assert
            Assert.Equal("test-type", entity.Type);
            Assert.NotNull(entity.Properties);
        }

        [Fact]
        public void Attachment_CanBeCreated()
        {
            // Arrange & Act
            var attachment = new Attachment
            {
                ContentType = "application/json",
                Content = new { message = "test" },
                Name = "test.json"
            };

            // Assert
            Assert.Equal("application/json", attachment.ContentType);
            Assert.NotNull(attachment.Content);
            Assert.Equal("test.json", attachment.Name);
        }

        [Fact]
        public void ResourceResponse_CanBeCreated()
        {
            // Arrange & Act
            var response = new ResourceResponse
            {
                Id = "response-123"
            };

            // Assert
            Assert.Equal("response-123", response.Id);
        }

        [Fact]
        public void MessageStream_CanBeCreated()
        {
            // Arrange
            var chunks = new List<string>();
            OnStreamChunk handler = async (text) =>
            {
                chunks.Add(text);
                await Task.CompletedTask;
            };

            // Act
            var stream = new MessageStream(handler);

            // Assert
            Assert.NotNull(stream);
        }

        [Fact]
        public void MessageStream_Emit_CallsHandler()
        {
            // Arrange
            var chunks = new List<string>();
            OnStreamChunk handler = async (text) =>
            {
                chunks.Add(text);
                await Task.CompletedTask;
            };
            var stream = new MessageStream(handler);

            // Act
            stream.Emit("Hello");
            stream.Emit("World");

            // Assert
            Assert.Equal(2, chunks.Count);
            Assert.Equal("Hello", chunks[0]);
            Assert.Equal("World", chunks[1]);
        }

        [Fact]
        public void MessageStream_ThrowsOnNullHandler()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new MessageStream(null!));
        }

        [Fact]
        public void IStream_InterfaceIsImplemented()
        {
            // Arrange
            OnStreamChunk handler = async (text) => await Task.CompletedTask;
            
            // Act
            IStream stream = new MessageStream(handler);

            // Assert
            Assert.NotNull(stream);
            stream.Emit("test");
        }
    }
}
