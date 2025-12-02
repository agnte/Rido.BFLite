using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Rido.BFLite.Core;

namespace Rido.BFLite.Compat.Adapter
{
    internal class CompatConversations(ConversationClient client) : IConversations
    {
        internal string? ServiceUrl { get; set; }
        public async Task<HttpOperationResponse<ConversationResourceResponse>> CreateConversationWithHttpMessagesAsync(ConversationParameters parameters, Dictionary<string, List<string>> customHeaders = null!, CancellationToken cancellationToken = default)
        {

            ConversationClient.CreateRequest createParams = new()
            {
                Bot = new()
                {
                    Id = parameters.Bot.Id,
                    Name = parameters.Bot.Name
                },
                IsGroup = parameters.IsGroup,
                Members = [.. parameters.Members.Select(m => new Rido.BFLite.Core.Schema.ConversationAccount()
                {
                    Id = m.Id,
                    Name = m.Name,
                    // AadObjectId = m.AadObjectId,
                    // Role = m.Role
                })],
            };

            ConversationClient.ConversationResource response = await client.CreateConversationAsync(ServiceUrl!, createParams, customHeaders, cancellationToken);
            return new HttpOperationResponse<ConversationResourceResponse>()
            {
                Body = new ConversationResourceResponse()
                {
                    Id = response.Id
                },
                // Response = response.Response
            };
        }

        public Task<HttpOperationResponse> DeleteActivityWithHttpMessagesAsync(string conversationId, string activityId, Dictionary<string, List<string>> customHeaders = null!, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<HttpOperationResponse> DeleteConversationMemberWithHttpMessagesAsync(string conversationId, string memberId, Dictionary<string, List<string>> customHeaders = null!, CancellationToken cancellationToken = default)
        {
            await client.DeleteConversationMemberAsync(ServiceUrl!, conversationId, memberId, customHeaders, cancellationToken);
            return new HttpOperationResponse();
        }

        public async Task<HttpOperationResponse<IList<ChannelAccount>>> GetActivityMembersWithHttpMessagesAsync(string conversationId, string activityId, Dictionary<string, List<string>> customHeaders = null!, CancellationToken cancellationToken = default)
        {
            IList<Core.Schema.ConversationAccount> members = await client.GetActivityMembersAsync(ServiceUrl!, conversationId, activityId, customHeaders, cancellationToken);
            return new HttpOperationResponse<IList<ChannelAccount>>()
            {
                Body = [.. members.Select(m => new ChannelAccount()
                {
                    Id = m.Id,
                    Name = m.Name
                })]
            };
        }

        public async Task<HttpOperationResponse<IList<ChannelAccount>>> GetConversationMembersWithHttpMessagesAsync(string conversationId, Dictionary<string, List<string>> customHeaders = null!, CancellationToken cancellationToken = default)
        {
            IList<Core.Schema.ConversationAccount> members = await client.GetConversationMembersAsync(ServiceUrl!, conversationId, customHeaders, cancellationToken);
            return new HttpOperationResponse<IList<ChannelAccount>>()
            {
                Body = [.. members.Select(m => new ChannelAccount()
                {
                    Id = m.Id,
                    Name = m.Name
                })]
            };
        }

        public async Task<HttpOperationResponse<Microsoft.Bot.Schema.PagedMembersResult>> GetConversationPagedMembersWithHttpMessagesAsync(string conversationId, int? pageSize = null, string continuationToken = null!, Dictionary<string, List<string>> customHeaders = null!, CancellationToken cancellationToken = default)
        {
            ConversationClient.PagedMembersResult pagedMembers = await client.GetConversationPagedMembersAsync(ServiceUrl!, conversationId, pageSize, continuationToken, customHeaders, cancellationToken);
            return new HttpOperationResponse<PagedMembersResult>()
            {
                Body = new PagedMembersResult()
                {
                    ContinuationToken = pagedMembers.ContinuationToken,
                    Members = [.. pagedMembers.Members!.Select(m => new ChannelAccount()
                    {
                        Id = m.Id,
                        Name = m.Name
                    })]
                }
            };
        }

        public async Task<HttpOperationResponse<ConversationsResult>> GetConversationsWithHttpMessagesAsync(string continuationToken = null!, Dictionary<string, List<string>> customHeaders = null!, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<ResourceResponse>> ReplyToActivityWithHttpMessagesAsync(string conversationId, string activityId, Activity activity, Dictionary<string, List<string>> customHeaders = null!, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<ResourceResponse>> SendConversationHistoryWithHttpMessagesAsync(string conversationId, Microsoft.Bot.Schema.Transcript transcript, Dictionary<string, List<string>> customHeaders = null!, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<ResourceResponse>> SendToConversationWithHttpMessagesAsync(string conversationId, Activity activity, Dictionary<string, List<string>> customHeaders = null!, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<ResourceResponse>> UpdateActivityWithHttpMessagesAsync(string conversationId, string activityId, Activity activity, Dictionary<string, List<string>> customHeaders = null!, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<ResourceResponse>> UploadAttachmentWithHttpMessagesAsync(string conversationId, Microsoft.Bot.Schema.AttachmentData attachmentUpload, Dictionary<string, List<string>> customHeaders = null!, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
