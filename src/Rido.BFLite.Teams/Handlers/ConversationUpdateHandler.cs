using Rido.BFLite.Core.Schema;
using Rido.BFLite.Teams.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rido.BFLite.Teams.Handlers
{
    // public Func<ConversationUpdateActivityWrapper, CancellationToken, Task>? OnConversationUpdate { get; set; }
    public delegate Task ConversationUpdateHandler(ConversationUpdateArgs conversationUpdateActivity, CancellationToken cancellationToken = default);

    public class ConversationUpdateArgs(TeamsActivity act)
    {
        public TeamsActivity Activity { get; set; } = act;

        public IList<ConversationAccount>? MembersAdded { get; set; } =
            act.Properties.TryGetValue("membersAdded", out object? value)
                && value is JsonElement je
                && je.ValueKind == JsonValueKind.Array
                    ? JsonSerializer.Deserialize<IList<ConversationAccount>>(je.GetRawText())
                    : null;

        public IList<ConversationAccount>? MembersRemoved { get; set; } =
            act.Properties.TryGetValue("membersRemoved", out object? value2)
                && value2 is JsonElement je2
                && je2.ValueKind == JsonValueKind.Array
                    ? JsonSerializer.Deserialize<IList<ConversationAccount>>(je2.GetRawText())
                    : null;
    }
}
