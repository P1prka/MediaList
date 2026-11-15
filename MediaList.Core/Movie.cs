namespace MediaList.Core;

public class Movie : MediaItem
{
    public Movie(string title, string genre, int year, MediaStatus status)
        : base(title, genre, year, status)
    {
    }

    public override string TypeName => "Movie";
}