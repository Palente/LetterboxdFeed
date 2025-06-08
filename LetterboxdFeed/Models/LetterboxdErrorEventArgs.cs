namespace LetterboxdFeed.Models;

public class LetterboxdErrorEventArgs(string username, Exception exception) : EventArgs
{
    public string Username { get; } = username;
    public Exception Exception { get; } = exception;
}