using System.Globalization;
using System.Text.RegularExpressions;
using System.Timers;
using System.Xml.Linq;
using LetterboxdFeed.Models;
using LetterboxdFeed.Options;
using Timer = System.Timers.Timer;
namespace LetterboxdFeed;

public partial class LetterboxdFeed : IDisposable
{
    private readonly LetterboxdFeedOptions _options;
    private Timer? _timer;
    private bool _firstRun = true;
    private readonly HashSet<string> _usernames = new();
    private readonly Dictionary<string, Media> _movieCache = new();
    
    /// <summary>
    /// Event triggered when a user watches a new movie.
    /// </summary>
    public event EventHandler<MovieWatchedEventArgs>? MovieWatched;
    /// <summary>
    /// Event triggered when an error occurs while processing the feed.
    /// </summary>
    public event EventHandler<LetterboxdErrorEventArgs>? ErrorOccurred;
    /// <summary>
    /// Event triggered when polling starts.
    /// </summary>
    public event EventHandler? PollingStarted;
    /// <summary>
    /// Event triggered when polling stops.
    /// </summary>
    public event EventHandler? PollingStopped;
    /// <summary>
    /// Get the usernames that are being watched.
    /// </summary>
    /// <returns></returns>
    public IReadOnlySet<string> GetUsernames() => _usernames.ToHashSet();
    
    public LetterboxdFeed(LetterboxdFeedOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="username">Username of the letterboxd account</param>
    /// <exception cref="ArgumentException"></exception>
    public void AddUsername(string username)
    {
        if(string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty!", nameof(username));
        _usernames.Add(username);
    }
    /// <summary>
    /// Remove a username from the watch list.
    /// </summary>
    /// <param name="username">Username of the letterboxd account</param>
    /// <exception cref="ArgumentException"></exception>
    public void RemoveUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty!", nameof(username));
        _usernames.Remove(username);
        _movieCache.Remove(username);
    }
    /// <summary>
    /// Add multiple usernames to watch.
    /// </summary>
    /// <param name="usernames">Usernames to watch</param>
    public void AddUsernames(IEnumerable<string> usernames)
    {
        ArgumentNullException.ThrowIfNull(usernames);
        foreach (var username in usernames)
        {
            AddUsername(username);
        }
    }
    /// <summary>
    /// Starts polling the Letterboxd feed for updates.
    /// </summary>
    public void StartPolling()
    {
        if (_timer is not null)
        {
            // Already polling, do nothing
            return;
        }
        // Autopolling is disabled, do nothing
        if(!_options.EnableAutoPolling) return;

        _timer = new Timer(_options.PollingInterval * 60 * 1000);
        _timer.Elapsed += OnTimerElapsed;
        _timer.Start();
        
        PollingStarted?.Invoke(this, EventArgs.Empty);
    }
    /// <summary>
    /// Stops the polling of the Letterboxd feed.
    /// </summary>
    public void StopPolling()
    {
        if (_timer is not null)
        {
            _timer.Stop();
            _timer.Dispose();
            _timer = null;
            
            PollingStopped?.Invoke(this, EventArgs.Empty);
        }
    }
    /// <summary>
    /// Get the latest movie for a specific user
    /// </summary>
    public async Task<Media?> GetLatestMovieAsync(string username)
    {
        var movies = await GetUserMoviesAsync(username);
        return movies?.OrderByDescending(x => x.PublishingDate).FirstOrDefault();
    }
    /// <summary>
    /// GetCached movie for a specific user, if it exists in the cache.
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public Media? GetCachedMovie(string username)
    {
        return _movieCache.GetValueOrDefault(username);
    }

    /// <summary>
    /// Get all movies for a specific user
    /// </summary>
    public async Task<List<Media>?> GetUserMoviesAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty!", nameof(username));
        return await RetrieveLetterboxdDataAsync(username);
    }
    private async Task ProcessAllUsersAsync()
    {
        foreach (var username in _usernames)
        {
            try
            {
                await ProcessUserAsync(username);
            }
            catch (Exception e)
            {
                ErrorOccurred?.Invoke(this, new LetterboxdErrorEventArgs(username, e));
            }
        }
    }

    private async Task ProcessUserAsync(string username)
    {
        var movie = await GetLatestMovieAsync(username);
        if (movie == null)
        {
            return;
        }

        // Check if this is a new movie
        if (_movieCache.TryGetValue(username, out var lastmovie))
        {
            if (lastmovie.MediaId == movie.MediaId)
            {
                // TODO: Check if the movie has been updated (e.g., new review, rewatch)
                return; // Same movie, no update
            }
        }
        _movieCache[username] = movie;
        // On the first run, just cache the movies without triggering events
        if (_firstRun)
        {
            return;
        }

        // Trigger the event
        MovieWatched?.Invoke(this, new MovieWatchedEventArgs(username, movie));
    }
    private async void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    { 
        await ProcessAllUsersAsync();
        if(_firstRun)
            _firstRun = false;
    }
    private async Task<List<Media>?> RetrieveLetterboxdDataAsync(string username)
    {
        try
        {
            var rssUrl = $"https://letterboxd.com/{username}/rss/";
            using var client = new HttpClient();
            var response = await client.GetStringAsync(rssUrl);
            var doc = XDocument.Parse(response);
            
            XNamespace letterboxd = "https://letterboxd.com";
            XNamespace tmdb = "https://themoviedb.org";
            
            var items = doc.Descendants().Where(x => x.Name.LocalName == "item");
            List<Media> movies = [];
            
            foreach (var item in items)
            {
                var movieIdElement = item.Element(tmdb + "movieId");
                var tvIdElement = item.Element(tmdb + "tvId");
                var isTvShow = movieIdElement == null && tvIdElement != null;
                var mediaId = int.Parse((movieIdElement ?? tvIdElement)?.Value ?? "-1");
                var isARewatch = item.Element(letterboxd + "rewatch")?.Value == "Yes";
                var title = item.Element(letterboxd + "filmTitle")?.Value ?? "Unknown";
                var filmYear = int.Parse(item.Element(letterboxd + "filmYear")?.Value ?? "-1");
                var rate = float.Parse(item.Element(letterboxd + "memberRating")?.Value ?? "-1", 
                    NumberStyles.Float, CultureInfo.InvariantCulture);
                var link = item.Element("link")?.Value ?? "Unknown";
                var watchedDateString = item.Element(letterboxd + "watchedDate")?.Value ?? "1971-01-01";
                var watchDate = DateOnly.Parse(watchedDateString, CultureInfo.InvariantCulture);
                var titleLetterboxd = item.Element("title")?.Value ?? "Unknown";
                var pubDateString = item.Element("pubDate")?.Value ?? "1971-01-01";
                var pubDate = DateTime.Parse(pubDateString, CultureInfo.InvariantCulture);
                var opinion = item.Element("description")?.Value ?? string.Empty;

                string? review = null;
                // Hack, if no opinion is given, rss return "Watched on {date}"
                if (!opinion.Contains("Watched on"))
                {
                    // User has given a review, time to clean it up
                    review = ExtractReviewText(opinion);
                }
                movies.Add(new Media(mediaId, title, filmYear, rate, link, watchDate, 
                    titleLetterboxd, pubDate, review, isARewatch, isTvShow));
            }
            
            return movies;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, new LetterboxdErrorEventArgs(username, ex));
            throw;
        }
    }

    private static string ExtractReviewText(string text)
    {
        var cleanedText = MyRegexToGetText().Replace(text, string.Empty);
        
        var matches = MyRegexText().Matches(cleanedText);
        var textBuilder = new System.Text.StringBuilder();

        foreach (Match match in matches)
        {
            textBuilder.AppendLine(match.Groups[1].Value.Trim());
        }

        return textBuilder.ToString().TrimEnd();
    } 
    public void Dispose()
    {
        _timer?.Dispose();
    }

    [GeneratedRegex(@"<p><img[^>]*></p>\s*", RegexOptions.IgnoreCase, "fr-FR")]
    private static partial Regex MyRegexToGetText();
    [GeneratedRegex(@"<p>(.*?)</p>", RegexOptions.Singleline)]
    private static partial Regex MyRegexText();
}