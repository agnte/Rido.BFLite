using Rido.BFLite.Core.Schema;

namespace Rido.BFLite.Core.Tests;

public class ConversationUpdateActivityTests
{
    [Fact]
    public void AsConversationUpdate_MembersAdded()
    {
        string json = """
        {
            "type": "conversationUpdate",
            "membersAdded": [
                {
                    "id": "user1",
                    "name": "User One"
                },
                {
                    "id": "bot1",
                    "name": "Bot One"
                }
            ]
        }
        """;
        Activity act = Activity.FromJsonString(json);
        Assert.NotNull(act);
        Assert.Equal("conversationUpdate", act.Type);

        ConversationUpdateActivityWrapper? cua = new(act);

        Assert.NotNull(cua);
        Assert.NotNull(cua.MembersAdded);
        Assert.Equal(2, cua.MembersAdded!.Count);
        Assert.Equal("user1", cua.MembersAdded[0].Id);
        Assert.Equal("User One", cua.MembersAdded[0].Name);
        Assert.Equal("bot1", cua.MembersAdded[1].Id);
        Assert.Equal("Bot One", cua.MembersAdded[1].Name);
    }

    [Fact]
    public void AsConversationUpdate_MembersRemoved()
    {
        string json = """
        {
            "type": "conversationUpdate",
            "membersRemoved": [
                {
                    "id": "user2",
                    "name": "User Two"
                }
            ]
        }
        """;
        Activity act = Activity.FromJsonString(json);
        Assert.NotNull(act);
        Assert.Equal("conversationUpdate", act.Type);

        ConversationUpdateActivityWrapper? cua = new(act);

        Assert.NotNull(cua);
        Assert.NotNull(cua.MembersRemoved);
        Assert.Single(cua.MembersRemoved!);
        Assert.Equal("user2", cua.MembersRemoved[0].Id);
        Assert.Equal("User Two", cua.MembersRemoved[0].Name);
    }

    [Fact]
    public void AsConversationUpdate_BothMembersAddedAndRemoved()
    {
        string json = """
        {
            "type": "conversationUpdate",
            "membersAdded": [
                {
                    "id": "newuser",
                    "name": "New User"
                }
            ],
            "membersRemoved": [
                {
                    "id": "olduser",
                    "name": "Old User"
                }
            ]
        }
        """;
        Activity act = Activity.FromJsonString(json);
        Assert.NotNull(act);
        Assert.Equal("conversationUpdate", act.Type);

        ConversationUpdateActivityWrapper? cua = new(act);

        Assert.NotNull(cua);
        Assert.NotNull(cua.MembersAdded);
        Assert.NotNull(cua.MembersRemoved);
        Assert.Single(cua.MembersAdded!);
        Assert.Single(cua.MembersRemoved!);
        Assert.Equal("newuser", cua.MembersAdded[0].Id);
        Assert.Equal("olduser", cua.MembersRemoved[0].Id);
    }
}
