using System.Collections.ObjectModel;

namespace MediaList.Core;

public class MediaLibrary
{
    public ObservableCollection<MediaItem> Items { get; } = new();

    public void Add(MediaItem item)
    {
        if (IsDuplicate(item, null))
        {
            throw new InvalidOperationException(
                $"\"{item.Title}\" ({item.Year}) is already in the list.");
        }

        Items.Add(item);
    }

    public void Replace(MediaItem oldItem, MediaItem newItem)
    {
        if (IsDuplicate(newItem, oldItem))
        {
            throw new InvalidOperationException(
                $"\"{newItem.Title}\" ({newItem.Year}) is already in the list.");
        }

        newItem.Id = oldItem.Id;

        if (oldItem.IsFavorite)
        {
            newItem.ToggleFavorite();
        }

        if (oldItem.Rating is int rating && newItem.Status == MediaStatus.Watched)
        {
            newItem.SetRating(rating);
        }

        int index = Items.IndexOf(oldItem);
        Items[index] = newItem;
    }

    public void Remove(MediaItem item)
    {
        Items.Remove(item);
    }

    public void Load(IEnumerable<MediaItem> items)
    {
        Items.Clear();

        foreach (MediaItem item in items)
        {
            Add(item);
        }
    }

    public int CountByStatus(MediaStatus status)
    {
        return Items.Count(i => i.Status == status);
    }

    public MediaItem? GetRandomToWatch()
    {
        List<MediaItem> list = Items
            .Where(i => i.Status == MediaStatus.WantToWatch)
            .ToList();

        if (list.Count == 0)
        {
            return null;
        }

        return list[Random.Shared.Next(list.Count)];
    }

    private bool IsDuplicate(MediaItem item, MediaItem? except)
    {
        return Items.Any(i =>
            !ReferenceEquals(i, except)
            && i.Title.Equals(item.Title, StringComparison.OrdinalIgnoreCase)
            && i.Year == item.Year);
    }
}