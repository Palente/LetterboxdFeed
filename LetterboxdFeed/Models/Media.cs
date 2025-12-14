namespace LetterboxdFeed.Models;

/// <summary>
/// Represents a media item in the Letterboxd feed.
/// </summary>
public record Media(
    int MediaId,
    string Title,
    int FilmYear,
    double Rate,
    string Link,
    DateOnly WatchedTime,
    string TitleLetterboxd,
    DateTime PublishingDate,
    string? Review = null,
    bool IsARewatch = false,
    bool IsTvShow = false)
{
    public virtual bool Equals(Media? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return MediaId == other.MediaId &&
               Title == other.Title &&
               FilmYear == other.FilmYear &&
               Rate.Equals(other.Rate) &&
               Link == other.Link &&
               WatchedTime.Equals(other.WatchedTime) &&
               TitleLetterboxd == other.TitleLetterboxd &&
               PublishingDate.Equals(other.PublishingDate) &&
               Review == other.Review &&
               IsARewatch == other.IsARewatch &&
               IsTvShow == other.IsTvShow;
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(MediaId);
        hashCode.Add(Title);
        hashCode.Add(FilmYear);
        hashCode.Add(Rate);
        hashCode.Add(Link);
        hashCode.Add(WatchedTime);
        hashCode.Add(TitleLetterboxd);
        hashCode.Add(PublishingDate);
        hashCode.Add(Review);
        hashCode.Add(IsARewatch);
        hashCode.Add(IsTvShow);
        return hashCode.ToHashCode();
    }
}