namespace Rido.BFLite.Core.Schema;

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
