using CliWrap;
using CliWrap.Buffered;
using TelegramBotYtMusic.Interfaces;

namespace TelegramBotYtMusic.Services;

public class AudioDownloaderService(ILogger<AudioDownloaderService> logger) : IAudioDownloaderService
{
    public async Task<string> DownloadAudioAsync(string query, string quality, CancellationToken cancellationToken)
    {
        string audioQualityArg = quality == "high" ? "0" : "5";
        
        var downloadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Downloads");
        if (!Directory.Exists(downloadsFolder)) Directory.CreateDirectory(downloadsFolder);

        var fileId = Guid.NewGuid().ToString();
        var templatePath = Path.Combine(downloadsFolder, $"{fileId}.%(ext)s");
        var finalFilePath = Path.Combine(downloadsFolder, $"{fileId}.mp3");
        
        var result = await Cli.Wrap("/usr/local/bin/yt-dlp")
            .WithArguments(args => args
                .Add("--cookies-from-browser").Add("chrome")
                .Add("-x")
                .Add("--audio-format").Add("mp3")
                .Add("--audio-quality").Add(audioQualityArg)
                .Add($"ytsearch1:{query}")
                .Add("-o").Add(templatePath))
            .WithValidation(CommandResultValidation.None) 
            .ExecuteBufferedAsync(cancellationToken);     // спаситель боженько
        
        if (result.ExitCode != 0)
        {
            throw new Exception($"Детали от yt-dlp:\n{result.StandardError}");
        }
        
        if (!File.Exists(finalFilePath)) 
            throw new Exception($"Файл скачался, но не найден: {finalFilePath}.");

        return finalFilePath;
    }
}