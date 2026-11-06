using System.IO;

namespace MediaPlayerApp.Models;

/// <summary>
/// Encapsulates information about a media item to be played.
/// </summary>
public sealed class MediaItem
{
    public Uri Uri { get; }
    public string Title { get; }
    public MediaType MediaType { get; }
    public string SourceDescription { get; }

    public MediaItem(Uri uri, string title, MediaType mediaType, string sourceDescription)
    {
        Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        Title = string.IsNullOrWhiteSpace(title) ? "Unknown Media" : title;
        MediaType = mediaType;
        SourceDescription = string.IsNullOrWhiteSpace(sourceDescription) ? uri.ToString() : sourceDescription;
    }

    public static MediaType DetectMediaType(string pathOrUrl)
    {
        if (string.IsNullOrWhiteSpace(pathOrUrl))
        {
            return MediaType.Unknown;
        }

        string cleanPath = pathOrUrl;
        int queryIndex = cleanPath.IndexOf('?');
        if (queryIndex >= 0)
        {
            cleanPath = cleanPath[..queryIndex];
        }

        string extension = Path.GetExtension(cleanPath).ToLowerInvariant();

        return extension switch
        {
            ".mp3" or ".wav" or ".aac" or ".flac" or ".wma" or ".m4a" or ".ogg" => MediaType.Audio,
            ".mp4" or ".avi" or ".mkv" or ".wmv" or ".mov" or ".webm" or ".m4v" => MediaType.Video,
            _ => MediaType.Unknown
        };
    }
}
