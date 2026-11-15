using System.Collections.ObjectModel;

namespace MediaList.Core;

public class MediaLibrary
{
    public ObservableCollection<MediaItem> Items { get; } = new();

    public void Add(MediaItem item)
    {
        bool exists = Items.Any(i =>
            i.Title.Equals(item.Title, StringComparison.OrdinalIgnoreCase)
            && i.Year == item.Year);

        if (exists)
        {
            throw new InvalidOperationException(
                $"\"{item.Title}\" ({item.Year}) is already in the list.");
        }

        Items.Add(item);
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
}

