using CliWrap;
using TelegramBotYtMusic.Entities;

namespace TelegramBotYtMusic.Services;

public class AudioDownloaderService(ILogger<AudioDownloaderService> logger) : IAudioDownloaderService
{
    public async Task<string> DownloadAudioAsync(string searchTerm, CancellationToken cancellationToken)
    {
        string searchQuery = searchTerm.StartsWith("http") ? searchTerm : $"ytsearch:{searchTerm}";
        
        var downloadPath = Path.Combine(AppContext.BaseDirectory, "Downloads");
        Directory.CreateDirectory(downloadPath);
        
        var fileTemplate = Path.Combine(downloadPath, "%(title)s.%(ext)s");
        
        var arguments = new List<string>
        {
            "--quiet",
            "--extract-audio",
            "--audio-format", "mp3",
            "--audio-quality", "0",
            "--no-mtime",
            "--default-search", "ytsearch",
            "--output", fileTemplate,
            searchQuery
        };
        
        await Cli.Wrap("yt-dlp")
            .WithArguments(arguments)
            .ExecuteAsync(cancellationToken);
        
        var directory = new DirectoryInfo(downloadPath);
        var downloadedFile = directory.GetFiles("*.mp3")
            .OrderByDescending(f => f.CreationTime)
            .FirstOrDefault(f => f.CreationTime >= DateTime.Now.AddSeconds(-10));

        if (downloadedFile == null) 
            throw new Exception($"Файл скачался, но не нашелся в папке {downloadPath}.");

        return downloadedFile.FullName;
    }
}