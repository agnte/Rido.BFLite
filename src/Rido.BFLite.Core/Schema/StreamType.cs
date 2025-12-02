namespace Rido.BFLite.Core.Schema;

/// <summary>
/// Used to handle streamed chunks of text
/// </summary>
/// <param name="text">The text chunk</param>
public delegate Task OnStreamChunk(string text);

/// <summary>
/// Represents a stream for sending incremental updates
/// </summary>
public interface IStream
{
    /// <summary>
    /// Emit a text chunk to the stream
    /// </summary>
    /// <param name="text">The text chunk to emit</param>
    void Emit(string text);
}

/// <summary>
/// The type of streaming message
/// </summary>
public static class StreamType
{
    /// <summary>
    /// An informative update
    /// </summary>
    public const string Informative = "informative";

    /// <summary>
    /// A streaming update with partial message text
    /// </summary>
    public const string Streaming = "streaming";

    /// <summary>
    /// The final message
    /// </summary>
    public const string Final = "final";
}
