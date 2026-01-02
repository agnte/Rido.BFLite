namespace Rido.BFLite.Core.Schema;

/// <summary>
/// A citation for content in a message
/// </summary>
public class Citation
{
    /// <summary>
    /// The title of the citation
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The content of the citation
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// The URL of the citation
    /// </summary>
    public string? Url { get; set; }
}

/// <summary>
/// A client-side citation appearance
/// </summary>
public class ClientCitationAppearance
{
    /// <summary>
    /// The name of the citation
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// An abstract or snippet from the citation
    /// </summary>
    public string? Abstract { get; set; }

    /// <summary>
    /// The URL of the citation
    /// </summary>
    public string? Url { get; set; }
}

/// <summary>
/// A citation formatted for the client
/// </summary>
public class ClientCitation
{
    /// <summary>
    /// The position of the citation in the message
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// The appearance of the citation
    /// </summary>
    public ClientCitationAppearance? Appearance { get; set; }
}
