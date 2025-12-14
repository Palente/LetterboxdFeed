# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-12-14

### Added
- Initial release
- RSS feed polling for Letterboxd user activity
- Event-driven architecture with `MovieWatched`, `ErrorOccurred`, `PollingStarted`, and `PollingStopped` events
- Support for multiple users monitoring
- Automatic polling with configurable intervals
- Manual data retrieval methods
- Movie and TV show support
- Rewatch detection
- Rating and review extraction
- Built-in caching to prevent duplicate notifications
- Comprehensive error handling

### Features
- `LetterboxdFeed` main class with disposable pattern
- `LetterboxdFeedOptions` for configuration
- `Media` model for movie/TV show data
- `MovieWatchedEventArgs` and `LetterboxdErrorEventArgs` for events
- Methods: `AddUsername`, `RemoveUsername`, `AddUsernames`, `StartPolling`, `StopPolling`, `GetLatestMovieAsync`, `GetUserMoviesAsync`, `GetCachedMovie`, `GetUsernames`

[1.0.0]: https://github.com/Palente/LetterboxdFeed/releases/tag/v1.0.0

