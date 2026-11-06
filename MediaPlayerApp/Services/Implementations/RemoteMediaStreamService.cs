using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using MediaPlayerApp.Services.Interfaces;

namespace MediaPlayerApp.Services.Implementations;

/// <summary>
/// Resolves remote media streams by downloading and caching them locally to ensure reliable playback.
/// </summary>
public sealed class RemoteMediaStreamService : IRemoteMediaStreamService
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(60)
    };

    private readonly string _cacheDirectory;

    public RemoteMediaStreamService()
    {
        _cacheDirectory = Path.Combine(Path.GetTempPath(), "MediaPlayerApp_Cache");
        if (!Directory.Exists(_cacheDirectory))
        {
            Directory.CreateDirectory(_cacheDirectory);
        }
    }

    public async Task<Uri> ResolveMediaUriAsync(Uri mediaUri, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mediaUri);

        if (mediaUri.IsFile || (mediaUri.Scheme != Uri.UriSchemeHttp && mediaUri.Scheme != Uri.UriSchemeHttps))
        {
            return mediaUri;
        }

        string extension = Path.GetExtension(mediaUri.AbsolutePath);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".mp4";
        }

        byte[] uriBytes = Encoding.UTF8.GetBytes(mediaUri.AbsoluteUri);
        string hash = Convert.ToHexString(SHA256.HashData(uriBytes))[..16];
        string localFileName = $"stream_{hash}{extension}";
        string localFilePath = Path.Combine(_cacheDirectory, localFileName);

        if (File.Exists(localFilePath) && new FileInfo(localFilePath).Length > 0)
        {
            return new Uri(localFilePath, UriKind.Absolute);
        }

        using HttpResponseMessage response = await HttpClient.GetAsync(mediaUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        string tempDownloadPath = localFilePath + ".part";
        await using (FileStream fileStream = new(tempDownloadPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await response.Content.CopyToAsync(fileStream, cancellationToken);
        }

        File.Move(tempDownloadPath, localFilePath, overwrite: true);
        return new Uri(localFilePath, UriKind.Absolute);
    }
}
