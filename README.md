# LetterboxdFeed

[![NuGet](https://img.shields.io/nuget/v/LetterboxdFeed.svg)](https://www.nuget.org/packages/LetterboxdFeed)
[![NuGet Downloads](https://img.shields.io/nuget/dt/LetterboxdFeed.svg)](https://www.nuget.org/packages/LetterboxdFeed)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A lightweight .NET library that retrieves and streams Letterboxd activity (reviews, ratings, diary entries) using an event-driven model.

## Installation

```bash
dotnet add package LetterboxdFeed
```

## Features

- 📺 Monitor Letterboxd RSS feeds for multiple users
- 🔔 Event-driven notifications for new movie watches
- ⚡ Automatic polling with configurable intervals
- 🎬 Support for movies and TV shows
- 🔄 Rewatch detection
- ⭐ Rating and review extraction
- 💾 Built-in caching to avoid duplicate notifications

## Quick Start

```csharp
using LetterboxdFeed;
using LetterboxdFeed.Options;

// Configure options
var options = new LetterboxdFeedOptions
{
    PollingInterval = 5, // Check every 5 minutes
    EnableAutoPolling = true
};

// Create feed instance
using var feed = new LetterboxdFeed(options);

// Add users to watch
feed.AddUsername("username1");
feed.AddUsername("username2");

// Subscribe to events
feed.MovieWatched += (sender, args) =>
{
    Console.WriteLine($"{args.Username} watched: {args.Movie.Title} ({args.Movie.FilmYear})");
    Console.WriteLine($"Rating: {args.Movie.Rate}/5");
    if (args.Movie.Review != null)
    {
        if (args.Movie.Review.ContainsSpoilers)
        {
            Console.WriteLine("Warning: This review may contain spoilers.");
        }
        Console.WriteLine($"Review: {args.Movie.Review.Text}");
    }
};

feed.ErrorOccurred += (sender, args) =>
{
    Console.WriteLine($"Error for {args.Username}: {args.Exception.Message}");
};

// Start polling
feed.StartPolling();

// Keep the application running
Console.WriteLine("Monitoring Letterboxd feeds. Press Enter to stop...");
Console.ReadLine();

feed.StopPolling();
```

## Manual Polling

You can also retrieve data manually without automatic polling:

```csharp
var options = new LetterboxdFeedOptions
{
    EnableAutoPolling = false
};

using var feed = new LetterboxdFeed(options);

// Get latest movie for a user
var latestMovie = await feed.GetLatestMovieAsync("username");
if (latestMovie != null)
{
    Console.WriteLine($"{latestMovie.Title} - {latestMovie.Rate}/5");
}

// Get all movies from feed
var allMovies = await feed.GetUserMoviesAsync("username");
foreach (var movie in allMovies)
{
    Console.WriteLine($"{movie.Title} ({movie.FilmYear})");
}
```

## API Reference

### LetterboxdFeedOptions

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `PollingInterval` | `int` | 5 | Polling interval in minutes |
| `EnableAutoPolling` | `bool` | true | Enable automatic polling |

### LetterboxdFeed Methods

- `AddUsername(string username)` - Add a user to watch
- `RemoveUsername(string username)` - Remove a user from watch list
- `AddUsernames(IEnumerable<string> usernames)` - Add multiple users
- `StartPolling()` - Start automatic polling
- `StopPolling()` - Stop automatic polling
- `GetLatestMovieAsync(string username)` - Get latest movie for user
- `GetUserMoviesAsync(string username)` - Get all movies from RSS feed
- `GetCachedMovie(string username)` - Get cached movie for user
- `GetUsernames()` - Get list of watched usernames

### Events

- `MovieWatched` - Triggered when a new movie is detected
- `ErrorOccurred` - Triggered when an error occurs
- `PollingStarted` - Triggered when polling starts
- `PollingStopped` - Triggered when polling stops

### Media Model

| Property | Type | Description |
|----------|------|-------------|
| `MediaId` | `int` | TMDB ID |
| `Title` | `string` | Movie/TV show title |
| `FilmYear` | `int` | Release year |
| `Rate` | `double` | User rating (0-5) |
| `Link` | `string` | Letterboxd entry URL |
| `WatchedTime` | `DateOnly` | Date watched |
| `TitleLetterboxd` | `string` | Full Letterboxd title |
| `PublishingDate` | `DateTime` | RSS publish date |
| `Review` | `Review?` | User review (record with Text and ContainsSpoilers properties) |
| `IsARewatch` | `bool` | Whether it's a rewatch |
| `IsTvShow` | `bool` | Whether it's a TV show |

## License

MIT License - see [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Links

- [GitHub Repository](https://github.com/Palente/LetterboxdFeed)
- [NuGet Package](https://www.nuget.org/packages/LetterboxdFeed)
