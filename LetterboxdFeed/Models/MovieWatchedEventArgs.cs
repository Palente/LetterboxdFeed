namespace LetterboxdFeed.Models;

public class MovieWatchedEventArgs(string username, Media movie) : EventArgs
{
    public string Username { get; } = username;
    public Media Movie { get; } = movie;
}