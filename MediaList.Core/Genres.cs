namespace MediaList.Core;

public static class Genres
{
    public static readonly string[] All =
    {
        "Action", "Comedy", "Drama", "Sci-fi", "Fantasy", "Horror",
        "Thriller", "Crime", "Romance", "Adventure", "Family", "Documentary"
    };

    public static bool IsValid(string? genre)
    {
        return genre is not null && All.Contains(genre);
    }
}