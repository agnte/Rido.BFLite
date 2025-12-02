using Rido.BFLite.Core.Schema;

namespace Rido.BFLite.Core;

/// <summary>
/// Streams text chunks to the client.
/// Provides a simple API for streaming responses, matching the Microsoft teams.net API.
/// </summary>
public class MessageStream : IStream
{
    private readonly OnStreamChunk _onChunk;

    /// <summary>
    /// Creates a new MessageStream instance
    /// </summary>
    /// <param name="onChunk">Callback to handle each text chunk</param>
    public MessageStream(OnStreamChunk onChunk)
    {
        _onChunk = onChunk ?? throw new ArgumentNullException(nameof(onChunk));
    }

    /// <summary>
    /// Emits a text chunk to the stream
    /// </summary>
    /// <param name="text">The text chunk to emit</param>
    public void Emit(string text)
    {
        _onChunk(text).GetAwaiter().GetResult();
    }
}
