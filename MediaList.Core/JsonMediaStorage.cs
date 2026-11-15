using System.Text.Json;
using System.Text.Json.Serialization;

namespace MediaList.Core;

public class JsonMediaStorage
{
    private static readonly JsonSerializerOptions options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public string FilePath { get; }

    public JsonMediaStorage(string filePath)
    {
        FilePath = filePath;
    }

    public async Task SaveAsync(IEnumerable<MediaItem> items)
    {
        string? folder = Path.GetDirectoryName(FilePath);

        if (!string.IsNullOrEmpty(folder))
        {
            Directory.CreateDirectory(folder);
        }

        List<MediaItemDto> dtos = items.Select(MediaMapper.ToDto).ToList();

        await using FileStream stream = File.Create(FilePath);
        await JsonSerializer.SerializeAsync(stream, dtos, options);
    }

    public async Task<List<MediaItem>> LoadAsync()
    {
        if (!File.Exists(FilePath))
        {
            return new List<MediaItem>();
        }

        await using FileStream stream = File.OpenRead(FilePath);

        List<MediaItemDto>? dtos =
            await JsonSerializer.DeserializeAsync<List<MediaItemDto>>(stream, options);

        return (dtos ?? new List<MediaItemDto>())
            .Select(MediaMapper.FromDto)
            .ToList();
    }
}

