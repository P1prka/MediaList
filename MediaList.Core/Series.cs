namespace MediaList.Core;

public class Series : MediaItem
{
    public List<int> EpisodesPerSeason { get; }

    private int currentSeason;

    public int CurrentSeason
    {
        get => currentSeason;
        private set
        {
            currentSeason = value;
            OnPropertyChanged(nameof(CurrentSeason));
            OnPropertyChanged(nameof(ProgressText));
            OnPropertyChanged(nameof(ProgressPercent));
        }
    }

    private int currentEpisode;

    public int CurrentEpisode
    {
        get => currentEpisode;
        private set
        {
            currentEpisode = value;
            OnPropertyChanged(nameof(CurrentEpisode));
            OnPropertyChanged(nameof(ProgressText));
            OnPropertyChanged(nameof(ProgressPercent));
        }
    }

    public override string TypeName => "Series";

    public int TotalEpisodes => EpisodesPerSeason.Sum();

    public int WatchedEpisodes
    {
        get
        {
            if (Status == MediaStatus.Watched)
            {
                return TotalEpisodes;
            }

            int watched = EpisodesPerSeason.Take(CurrentSeason - 1).Sum();
            return watched + CurrentEpisode - 1;
        }
    }

    public int ProgressPercent => TotalEpisodes == 0 ? 0 : WatchedEpisodes * 100 / TotalEpisodes;

    public string ProgressText =>
        Status == MediaStatus.Watched
            ? "All seasons watched"
            : $"S{CurrentSeason} · E{CurrentEpisode}/{EpisodesPerSeason[CurrentSeason - 1]}";

    public Series(string title, string genre, int year, MediaStatus status,
        List<int> episodesPerSeason, int currentSeason = 1, int currentEpisode = 1)
        : base(title, genre, year, status)
    {
        if (episodesPerSeason.Count == 0)
        {
            throw new ArgumentException("Series must have at least 1 season.");
        }

        if (episodesPerSeason.Any(e => e < 1))
        {
            throw new ArgumentException("Each season must have at least 1 episode.");
        }

        if (currentSeason < 1 || currentSeason > episodesPerSeason.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(currentSeason), "Season does not exist.");
        }

        if (currentEpisode < 1 || currentEpisode > episodesPerSeason[currentSeason - 1])
        {
            throw new ArgumentOutOfRangeException(nameof(currentEpisode),
                $"Season {currentSeason} has only {episodesPerSeason[currentSeason - 1]} episodes.");
        }

        EpisodesPerSeason = episodesPerSeason;
        CurrentSeason = currentSeason;
        CurrentEpisode = currentEpisode;
    }

    public void StartWatching()
    {
        if (Status == MediaStatus.WantToWatch)
        {
            Status = MediaStatus.Watching;
            CurrentSeason = 1;
            CurrentEpisode = 1;
        }
    }

    public void NextEpisode()
    {
        if (Status == MediaStatus.Watched)
        {
            return;
        }

        Status = MediaStatus.Watching;

        if (CurrentEpisode < EpisodesPerSeason[CurrentSeason - 1])
        {
            CurrentEpisode++;
        }
        else if (CurrentSeason < EpisodesPerSeason.Count)
        {
            CurrentSeason++;
            CurrentEpisode = 1;
        }
        else
        {
            MarkWatched();
        }
    }

    public void NextSeason()
    {
        if (Status == MediaStatus.Watched)
        {
            return;
        }

        Status = MediaStatus.Watching;

        if (CurrentSeason < EpisodesPerSeason.Count)
        {
            CurrentSeason++;
            CurrentEpisode = 1;
        }
        else
        {
            MarkWatched();
        }
    }

    public override void MarkWatched()
    {
        base.MarkWatched();
        OnPropertyChanged(nameof(ProgressText));
        OnPropertyChanged(nameof(ProgressPercent));
    }
}