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
        }
    }

    public abstract string TypeName { get; }

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