using System.ComponentModel;

namespace MediaList.Core;

public abstract class MediaItem : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public Guid Id { get; internal set; } = Guid.NewGuid();

    public string Title { get; }

    public string Genre { get; }

    public int Year { get; }

    private MediaStatus status;

    public MediaStatus Status
    {
        get => status;
        protected set
        {
            status = value;
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(IsWatched));
            OnPropertyChanged(nameof(HasMainAction));
            OnPropertyChanged(nameof(MainActionText));
            OnPropertyChanged(nameof(HasNextSeason));
            OnPropertyChanged(nameof(ShowProgress));
            OnPropertyChanged(nameof(ProgressText));
            OnPropertyChanged(nameof(ProgressPercent));
        }
    }

    private int? rating;

    public int? Rating
    {
        get => rating;
        private set
        {
            rating = value;
            OnPropertyChanged(nameof(Rating));
            OnPropertyChanged(nameof(StarsText));
        }
    }

    private bool isFavorite;

    public bool IsFavorite
    {
        get => isFavorite;
        private set
        {
            isFavorite = value;
            OnPropertyChanged(nameof(IsFavorite));
            OnPropertyChanged(nameof(FavoriteText));
        }
    }

    public abstract string TypeName { get; }

    public string InfoText => $"{Genre} · {Year}";

    public bool IsWatched => Status == MediaStatus.Watched;

    public bool HasMainAction => Status != MediaStatus.Watched;

    public virtual string MainActionText => "✓ Watched";

    public virtual bool HasNextSeason => false;

    public virtual bool ShowProgress => false;

    public virtual string ProgressText => "";

    public virtual int ProgressPercent => 0;

    public string StarsText =>
        Rating is int r ? new string('★', r) + new string('☆', 5 - r) : "☆☆☆☆☆";

    public string FavoriteText => IsFavorite ? "♥" : "♡";

    protected MediaItem(string title, string genre, int year, MediaStatus status)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.");
        }

        if (!Genres.IsValid(genre))
        {
            throw new ArgumentException("Genre must be chosen from the list.");
        }

        if (year < 1900 || year > DateTime.Now.Year)
        {
            throw new ArgumentOutOfRangeException(
                nameof(year), $"Year must be between 1900 and {DateTime.Now.Year}.");
        }

        Title = title.Trim();
        Genre = genre;
        Year = year;
        Status = status;
    }

    public virtual void MarkWatched()
    {
        Status = MediaStatus.Watched;
    }

    public virtual void DoMainAction()
    {
        MarkWatched();
    }

    public void SetRating(int value)
    {
        if (Status != MediaStatus.Watched)
        {
            throw new InvalidOperationException("Only watched titles can be rated.");
        }

        if (value < 1 || value > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Rating must be between 1 and 5.");
        }

        Rating = value;
    }

    public void NextRating()
    {
        if (Rating is int r && r < 5)
        {
            SetRating(r + 1);
        }
        else
        {
            SetRating(1);
        }
    }

    public void ToggleFavorite()
    {
        IsFavorite = !IsFavorite;
    }

    public override string ToString()
    {
        return $"{TypeName}: {Title} ({Year})";
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
