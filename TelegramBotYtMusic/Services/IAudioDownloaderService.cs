namespace TelegramBotYtMusic.Services;

public interface IAudioDownloaderService
{
    Task<string> DownloadAudioAsync(string searchTerm, CancellationToken cancellationToken);
}