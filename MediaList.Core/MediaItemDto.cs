namespace MediaList.Core;

public class MediaItemDto
{
    public Guid Id { get; set; }

    public string Type { get; set; } = "";

    public string Title { get; set; } = "";

    public string Genre { get; set; } = "";

    public int Year { get; set; }

    public MediaStatus Status { get; set; }

    public int? Rating { get; set; }

    public bool IsFavorite { get; set; }

    public List<int>? EpisodesPerSeason { get; set; }

    public int CurrentSeason { get; set; } = 1;

    public int CurrentEpisode { get; set; } = 1;
}
