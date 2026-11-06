using System.IO;
using MediaPlayerApp.Models;
using MediaPlayerApp.Services.Interfaces;

namespace MediaPlayerApp.Services.Implementations;

/// <summary>
/// Provides access to bundled sample media files for verification and testing.
/// </summary>
public sealed class SampleProviderService : ISampleProviderService
{
    public IReadOnlyList<MediaItem> GetSampleMediaItems()
    {
        var items = new List<MediaItem>();
        string baseDir = AppContext.BaseDirectory;
        string samplesDir = Path.Combine(baseDir, "Samples");

        if (!Directory.Exists(samplesDir))
        {
            string projectDir = Directory.GetCurrentDirectory();
            string candidate = Path.Combine(projectDir, "Samples");
            if (Directory.Exists(candidate))
            {
                samplesDir = candidate;
            }
        }

        if (Directory.Exists(samplesDir))
        {
            string[] files = Directory.GetFiles(samplesDir);
            Array.Sort(files);

            foreach (string filePath in files)
            {
                string fileName = Path.GetFileName(filePath);
                MediaType mediaType = MediaItem.DetectMediaType(fileName);
                string title = FormatSampleTitle(fileName);
                string description = GetSampleDescription(fileName);
                var uri = new Uri(filePath, UriKind.Absolute);
                items.Add(new MediaItem(uri, title, mediaType, description));
            }
        }

        return items.AsReadOnly();
    }

    private static string FormatSampleTitle(string fileName)
    {
        return fileName switch
        {
            "SampleAudio_Chime.wav" => "Chime (WAV Audio)",
            "SampleAudio_Music.mp3" => "Acoustic Melody (MP3 Audio)",
            "SampleVideo_BBB.mp4" => "Big Buck Bunny (MP4 Video)",
            "SampleVideo_Beach.avi" => "Woman on the Beach (AVI Video)",
            "SampleVideo_Car.avi" => "Woman on the Beach (AVI Video)",
            _ => fileName
        };
    }

    private static string GetSampleDescription(string fileName)
    {
        return fileName switch
        {
            "SampleAudio_Chime.wav" => "System notification sound (WAV format)",
            "SampleAudio_Music.mp3" => "Acoustic guitar music track (MP3 format)",
            "SampleVideo_BBB.mp4" => "Blender Foundation animated short (MP4 format)",
            "SampleVideo_Beach.avi" => "Woman sitting on the beach (AVI format)",
            "SampleVideo_Car.avi" => "Woman sitting on the beach (AVI format)",
            _ => "Bundled Sample Media"
        };
    }
}
