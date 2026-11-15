using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using MediaList.Core;

namespace MediaList.WpfApp;

public class MainViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly MediaLibrary library = new();
    private readonly JsonMediaStorage storage;

    public ObservableCollection<MediaItem> VisibleItems { get; } = new();
    public ObservableCollection<SeasonInput> SeasonInputs { get; } = new();

    public string[] GenreOptions { get; } = Genres.All;
    public string[] StatusOptions { get; } = { "Want to watch", "Watching", "Watched" };
    public string[] SortOptions { get; } = { "Title A-Z", "Year (newest)", "Rating (best)" };

    public RelayCommand MainActionCommand { get; }
    public RelayCommand NextSeasonCommand { get; }
    public RelayCommand RateCommand { get; }
    public RelayCommand FavoriteCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand RandomCommand { get; }
    public RelayCommand ShowAddCommand { get; }
    public RelayCommand CancelAddCommand { get; }
    public RelayCommand SaveNewCommand { get; }

    private string typeFilter = "All";
    private MediaStatus statusFilter = MediaStatus.Watching;

    public bool ShowAll
    {
        get => typeFilter == "All";
        set { if (value) SetTypeFilter("All"); }
    }

    public bool ShowMovies
    {
        get => typeFilter == "Movie";
        set { if (value) SetTypeFilter("Movie"); }
    }

    public bool ShowSeries
    {
        get => typeFilter == "Series";
        set { if (value) SetTypeFilter("Series"); }
    }

    public bool ShowCartoons
    {
        get => typeFilter == "Cartoon";
        set { if (value) SetTypeFilter("Cartoon"); }
    }

    public bool ShowFavorites
    {
        get => typeFilter == "Favorites";
        set { if (value) SetTypeFilter("Favorites"); }
    }

    public bool ShowWatching
    {
        get => statusFilter == MediaStatus.Watching;
        set { if (value) SetStatusFilter(MediaStatus.Watching); }
    }

    public bool ShowWantToWatch
    {
        get => statusFilter == MediaStatus.WantToWatch;
        set { if (value) SetStatusFilter(MediaStatus.WantToWatch); }
    }

    public bool ShowWatched
    {
        get => statusFilter == MediaStatus.Watched;
        set { if (value) SetStatusFilter(MediaStatus.Watched); }
    }

    public int AllCount => library.Items.Count;
    public int MoviesCount => library.Items.Count(i => i is Movie);
    public int SeriesCount => library.Items.Count(i => i is Series);
    public int CartoonsCount => library.Items.Count(i => i is Cartoon);
    public int FavoritesCount => library.Items.Count(i => i.IsFavorite);

    public string WatchingTabText => $"Watching ({CountForTab(MediaStatus.Watching)})";
    public string WantToWatchTabText => $"Want to watch ({CountForTab(MediaStatus.WantToWatch)})";
    public string WatchedTabText => $"Watched ({CountForTab(MediaStatus.Watched)})";

    public bool IsListEmpty => VisibleItems.Count == 0;

    private string searchText = "";

    public string SearchText
    {
        get => searchText;
        set
        {
            searchText = value ?? "";
            OnPropertyChanged(nameof(SearchText));
            RefreshList();
        }
    }

    private string sortMode = "Title A-Z";

    public string SortMode
    {
        get => sortMode;
        set
        {
            sortMode = value ?? "Title A-Z";
            OnPropertyChanged(nameof(SortMode));
            RefreshList();
        }
    }

    private string randomText = "";

    public string RandomText
    {
        get => randomText;
        set
        {
            randomText = value;
            OnPropertyChanged(nameof(RandomText));
        }
    }

    private string message = "";

    public string Message
    {
        get => message;
        set
        {
            message = value;
            OnPropertyChanged(nameof(Message));
        }
    }

    private bool isAddMode;

    public bool IsAddMode
    {
        get => isAddMode;
        set
        {
            isAddMode = value;
            OnPropertyChanged(nameof(IsAddMode));
            OnPropertyChanged(nameof(IsListMode));
        }
    }

    public bool IsListMode => !IsAddMode;

    private MediaItem? editingItem;

    public string FormTitle => editingItem is null ? "Add new title" : $"Edit: {editingItem.Title}";

    private string newType = "Movie";

    public bool IsNewMovie
    {
        get => newType == "Movie";
        set { if (value) SetNewType("Movie"); }
    }

    public bool IsNewSeries
    {
        get => newType == "Series";
        set { if (value) SetNewType("Series"); }
    }

    public bool IsNewCartoon
    {
        get => newType == "Cartoon";
        set { if (value) SetNewType("Cartoon"); }
    }

    private string newTitle = "";

    public string NewTitle
    {
        get => newTitle;
        set
        {
            newTitle = value ?? "";
            OnPropertyChanged(nameof(NewTitle));
        }
    }

    private string newGenre = Genres.All[0];

    public string NewGenre
    {
        get => newGenre;
        set
        {
            newGenre = value ?? Genres.All[0];
            OnPropertyChanged(nameof(NewGenre));
        }
    }

    private string newYearText = "";

    public string NewYearText
    {
        get => newYearText;
        set
        {
            newYearText = value ?? "";
            OnPropertyChanged(nameof(NewYearText));
        }
    }

    private string newStatus = "Want to watch";

    public string NewStatus
    {
        get => newStatus;
        set
        {
            newStatus = value ?? "Want to watch";
            OnPropertyChanged(nameof(NewStatus));
            OnPropertyChanged(nameof(ShowCurrentPosition));
        }
    }

    public bool ShowCurrentPosition => IsNewSeries && NewStatus == "Watching";

    private string seasonsCountText = "1";

    public string SeasonsCountText
    {
        get => seasonsCountText;
        set
        {
            seasonsCountText = value ?? "";
            OnPropertyChanged(nameof(SeasonsCountText));
            RebuildSeasonInputs();
        }
    }

    private string currentSeasonText = "1";

    public string CurrentSeasonText
    {
        get => currentSeasonText;
        set
        {
            currentSeasonText = value ?? "";
            OnPropertyChanged(nameof(CurrentSeasonText));
        }
    }

    private string currentEpisodeText = "1";

    public string CurrentEpisodeText
    {
        get => currentEpisodeText;
        set
        {
            currentEpisodeText = value ?? "";
            OnPropertyChanged(nameof(CurrentEpisodeText));
        }
    }

    private string errorText = "";

    public string ErrorText
    {
        get => errorText;
        set
        {
            errorText = value;
            OnPropertyChanged(nameof(ErrorText));
        }
    }

    public MainViewModel()
    {
        string folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MediaList");
        storage = new JsonMediaStorage(Path.Combine(folder, "media.json"));

        MainActionCommand = new RelayCommand(p => DoWithItem(p, item => item.DoMainAction()));
        NextSeasonCommand = new RelayCommand(p => DoWithItem(p, item => (item as Series)?.NextSeason()));
        RateCommand = new RelayCommand(p => DoWithItem(p, item => item.NextRating()));
        FavoriteCommand = new RelayCommand(p => DoWithItem(p, item => item.ToggleFavorite()));
        DeleteCommand = new RelayCommand(p => DoWithItem(p, item => library.Remove(item)));
        EditCommand = new RelayCommand(OpenEditForm);

        RandomCommand = new RelayCommand(PickRandom);
        ShowAddCommand = new RelayCommand(OpenAddForm);
        CancelAddCommand = new RelayCommand(() => IsAddMode = false);
        SaveNewCommand = new RelayCommand(SaveNewItem);

        RebuildSeasonInputs();
    }

    public async Task LoadAsync()
    {
        try
        {
            if (File.Exists(storage.FilePath))
            {
                library.Load(await storage.LoadAsync());
            }
            else
            {
                AddSampleData();
            }
        }
        catch (Exception ex)
        {
            Message = $"Could not load data: {ex.Message}";
        }

        RefreshList();
    }

    private void AddSampleData()
    {
        library.Add(new Series("Stranger Things", "Sci-fi", 2016, MediaStatus.Watching,
            new List<int> { 8, 9, 8, 9 }, 2, 5));
        library.Add(new Movie("Dune: Part Two", "Sci-fi", 2024, MediaStatus.WantToWatch));
        library.Add(new Cartoon("Madagascar", "Comedy", 2005, MediaStatus.Watched));
    }

    private void SetTypeFilter(string filter)
    {
        typeFilter = filter;
        OnPropertyChanged(nameof(ShowAll));
        OnPropertyChanged(nameof(ShowMovies));
        OnPropertyChanged(nameof(ShowSeries));
        OnPropertyChanged(nameof(ShowCartoons));
        OnPropertyChanged(nameof(ShowFavorites));
        RefreshList();
    }

    private void SetStatusFilter(MediaStatus status)
    {
        statusFilter = status;
        OnPropertyChanged(nameof(ShowWatching));
        OnPropertyChanged(nameof(ShowWantToWatch));
        OnPropertyChanged(nameof(ShowWatched));
        RefreshList();
    }

    private bool MatchesType(MediaItem item)
    {
        return typeFilter switch
        {
            "Movie" => item is Movie,
            "Series" => item is Series,
            "Cartoon" => item is Cartoon,
            "Favorites" => item.IsFavorite,
            _ => true
        };
    }

    private int CountForTab(MediaStatus status)
    {
        return library.Items.Count(i => MatchesType(i) && i.Status == status);
    }

    private void RefreshList()
    {
        IEnumerable<MediaItem> items = library.Items
            .Where(MatchesType)
            .Where(i => i.Status == statusFilter);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            items = items.Where(i =>
                i.Title.Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        items = SortMode switch
        {
            "Year (newest)" => items.OrderByDescending(i => i.Year),
            "Rating (best)" => items.OrderByDescending(i => i.Rating ?? 0),
            _ => items.OrderBy(i => i.Title)
        };

        VisibleItems.Clear();

        foreach (MediaItem item in items)
        {
            VisibleItems.Add(item);
        }

        OnPropertyChanged(nameof(IsListEmpty));
        OnPropertyChanged(nameof(AllCount));
        OnPropertyChanged(nameof(MoviesCount));
        OnPropertyChanged(nameof(SeriesCount));
        OnPropertyChanged(nameof(CartoonsCount));
        OnPropertyChanged(nameof(FavoritesCount));
        OnPropertyChanged(nameof(WatchingTabText));
        OnPropertyChanged(nameof(WantToWatchTabText));
        OnPropertyChanged(nameof(WatchedTabText));
    }

    private void DoWithItem(object? parameter, Action<MediaItem> action)
    {
        if (parameter is not MediaItem item)
        {
            return;
        }

        try
        {
            action(item);
            Message = "";
        }
        catch (InvalidOperationException ex)
        {
            Message = ex.Message;
        }

        RefreshList();
        _ = SaveAsync();
    }

    private void PickRandom()
    {
        MediaItem? pick = library.GetRandomToWatch();

        RandomText = pick is null
            ? "Nothing in \"Want to watch\" yet."
            : $"Tonight: {pick.Title} ({pick.Year})";
    }

    private void OpenAddForm()
    {
        editingItem = null;
        OnPropertyChanged(nameof(FormTitle));
        SetNewType("Movie");
        NewTitle = "";
        NewGenre = Genres.All[0];
        NewYearText = "";
        NewStatus = "Want to watch";
        SeasonsCountText = "1";
        CurrentSeasonText = "1";
        CurrentEpisodeText = "1";
        ErrorText = "";
        IsAddMode = true;
    }

    private void OpenEditForm(object? parameter)
    {
        if (parameter is not MediaItem item)
        {
            return;
        }

        editingItem = item;
        OnPropertyChanged(nameof(FormTitle));

        SetNewType(item.TypeName);
        NewTitle = item.Title;
        NewGenre = item.Genre;
        NewYearText = item.Year.ToString();
        NewStatus = item.Status switch
        {
            MediaStatus.Watching => "Watching",
            MediaStatus.Watched => "Watched",
            _ => "Want to watch"
        };

        if (item is Series series)
        {
            SeasonInputs.Clear();

            for (int i = 0; i < series.EpisodesPerSeason.Count; i++)
            {
                SeasonInputs.Add(new SeasonInput(i + 1)
                {
                    EpisodesText = series.EpisodesPerSeason[i].ToString()
                });
            }

            seasonsCountText = series.EpisodesPerSeason.Count.ToString();
            OnPropertyChanged(nameof(SeasonsCountText));
            CurrentSeasonText = series.CurrentSeason.ToString();
            CurrentEpisodeText = series.CurrentEpisode.ToString();
        }

        ErrorText = "";
        IsAddMode = true;
    }

    private void SetNewType(string type)
    {
        newType = type;
        OnPropertyChanged(nameof(IsNewMovie));
        OnPropertyChanged(nameof(IsNewSeries));
        OnPropertyChanged(nameof(IsNewCartoon));
        OnPropertyChanged(nameof(ShowCurrentPosition));
        ErrorText = "";
    }

    private void RebuildSeasonInputs()
    {
        if (!int.TryParse(SeasonsCountText, out int count) || count < 1 || count > 30)
        {
            return;
        }

        while (SeasonInputs.Count < count)
        {
            SeasonInputs.Add(new SeasonInput(SeasonInputs.Count + 1));
        }

        while (SeasonInputs.Count > count)
        {
            SeasonInputs.RemoveAt(SeasonInputs.Count - 1);
        }
    }

    private void SaveNewItem()
    {
        ErrorText = "";

        if (!int.TryParse(NewYearText, out int year))
        {
            ErrorText = "Year must be a number.";
            return;
        }

        MediaStatus status = NewStatus switch
        {
            "Watching" => MediaStatus.Watching,
            "Watched" => MediaStatus.Watched,
            _ => MediaStatus.WantToWatch
        };

        MediaItem item;

        try
        {
            if (IsNewSeries)
            {
                if (!int.TryParse(SeasonsCountText, out int seasonsCount) || seasonsCount < 1)
                {
                    ErrorText = "Series must have at least 1 season.";
                    return;
                }

                List<int> episodes = new();

                foreach (SeasonInput season in SeasonInputs)
                {
                    if (!int.TryParse(season.EpisodesText, out int count))
                    {
                        ErrorText = $"Episodes for {season.Label} must be a number.";
                        return;
                    }

                    episodes.Add(count);
                }

                int currentSeason = 1;
                int currentEpisode = 1;

                if (status == MediaStatus.Watching)
                {
                    if (!int.TryParse(CurrentSeasonText, out currentSeason)
                        || !int.TryParse(CurrentEpisodeText, out currentEpisode))
                    {
                        ErrorText = "Current season and episode must be numbers.";
                        return;
                    }
                }

                item = new Series(NewTitle, NewGenre, year, status,
                    episodes, currentSeason, currentEpisode);
            }
            else if (IsNewCartoon)
            {
                item = new Cartoon(NewTitle, NewGenre, year, status);
            }
            else
            {
                item = new Movie(NewTitle, NewGenre, year, status);
            }

            if (editingItem is null)
            {
                library.Add(item);
            }
            else
            {
                library.Replace(editingItem, item);
            }
        }
        catch (ArgumentException ex)
        {
            ErrorText = CleanMessage(ex.Message);
            return;
        }
        catch (InvalidOperationException ex)
        {
            ErrorText = ex.Message;
            return;
        }

        Message = editingItem is null ? $"Added {item.Title}." : $"Saved {item.Title}.";
        editingItem = null;
        IsAddMode = false;
        SetStatusFilter(status);
        _ = SaveAsync();
    }

    private static string CleanMessage(string text)
    {
        int index = text.IndexOf(" (Parameter", StringComparison.Ordinal);
        return index >= 0 ? text.Substring(0, index) : text;
    }

    private async Task SaveAsync()
    {
        try
        {
            await storage.SaveAsync(library.Items.ToList());
        }
        catch (Exception ex)
        {
            Message = $"Could not save: {ex.Message}";
        }
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}