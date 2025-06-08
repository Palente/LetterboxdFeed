namespace LetterboxdFeed.Options;

/// <summary>
/// Configuration options for the Letterboxd feed.
/// </summary>
public class LetterboxdFeedOptions
{
    /// <summary>
    /// Polling interval in minutes for the Letterboxd feed.
    /// </summary>
    public int PollingInterval { get; set; } = 5;
    /// <summary>
    /// Enable automatic polling of the Letterboxd feed.
    /// </summary>
    public bool EnableAutoPolling { get; set; } = true;
}