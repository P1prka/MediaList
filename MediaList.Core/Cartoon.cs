namespace MediaList.Core;

public class Cartoon : MediaItem
{
    public Cartoon(string title, string genre, int year, MediaStatus status)
        : base(title, genre, year, status)
    {
    }

    public override string TypeName => "Cartoon";
}