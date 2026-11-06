namespace MediaPlayerApp.Services.Interfaces;

/// <summary>
/// Defines the contract for resolving and caching remote media streams for reliable playback.
/// </summary>
public interface IRemoteMediaStreamService
{
    Task<Uri> ResolveMediaUriAsync(Uri mediaUri, CancellationToken cancellationToken = default);
}
