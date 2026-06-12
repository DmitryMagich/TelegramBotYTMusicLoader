namespace TelegramBotYtMusic.Interfaces;

public interface IAudioDownloaderService
{
    Task<string> DownloadAudioAsync(string searchTerm, string quality, CancellationToken cancellationToken);
}