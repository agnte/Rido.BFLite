using Rido.BFLite.Core.Schema;

namespace Rido.BFLite.Core.Tests;

public class MessageReactionActivityTests
{
    [Fact]
    public void AsMessageReaction()
    {
        string json = """
        {
            "type": "messageReaction",
            "reactionsAdded": [
                {
                    "type": "like"
                },
                {
                    "type": "heart"
                }
            ]
        }
        """;
        Activity act = Activity.FromJsonString(json);
        Assert.NotNull(act);
        Assert.Equal("messageReaction", act.Type);

        // MessageReactionActivity? mra = MessageReactionActivity.FromActivity(act);
        MessageReactionActivityWrapper? mra = new(act);

        Assert.NotNull(mra);
        Assert.NotNull(mra!.ReactionsAdded);
        Assert.Equal(2, mra!.ReactionsAdded!.Count);
        Assert.Equal("like", mra!.ReactionsAdded[0].Type);
        Assert.Equal("heart", mra!.ReactionsAdded[1].Type);
    }
}
