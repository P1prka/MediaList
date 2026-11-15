namespace MediaList.Core;

public static class MediaMapper
{
    public static MediaItemDto ToDto(MediaItem item)
    {
        MediaItemDto dto = new()
        {
            Id = item.Id,
            Type = item.TypeName,
            Title = item.Title,
            Genre = item.Genre,
            Year = item.Year,
            Status = item.Status,
            Rating = item.Rating,
            IsFavorite = item.IsFavorite
        };

        if (item is Series series)
        {
            dto.EpisodesPerSeason = series.EpisodesPerSeason.ToList();
            dto.CurrentSeason = series.CurrentSeason;
            dto.CurrentEpisode = series.CurrentEpisode;
        }

        return dto;
    }

    public static MediaItem FromDto(MediaItemDto dto)
    {
        MediaItem item = dto.Type switch
        {
            "Movie" => new Movie(dto.Title, dto.Genre, dto.Year, dto.Status),
            "Cartoon" => new Cartoon(dto.Title, dto.Genre, dto.Year, dto.Status),
            "Series" => new Series(dto.Title, dto.Genre, dto.Year, dto.Status,
                dto.EpisodesPerSeason ?? new List<int>(), dto.CurrentSeason, dto.CurrentEpisode),
            _ => throw new InvalidDataException($"Unknown type: {dto.Type}")
        };

        item.Id = dto.Id;

        if (dto.Rating is int rating)
        {
            item.SetRating(rating);
        }

        if (dto.IsFavorite)
        {
            item.ToggleFavorite();
        }

        return item;
    }
}
