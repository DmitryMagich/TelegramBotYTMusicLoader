using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotYtMusic.Database;
using TelegramBotYtMusic.Entities;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotYtMusic.Interfaces;


namespace TelegramBotYtMusic.Services;

public class MusicSearchService(
    ILogger<MusicSearchService> logger,
    IAudioDownloaderService audioDownloaderService,
    IServiceScopeFactory scopeFactory) : IMusicSearchService
{
    public async Task MusicSearchAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        var messageText = message.Text!;

        logger.LogInformation("Запрос: {MessageText}", messageText);

        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();

        var cachedTrack = await dbContext.TrackCaches
            .FirstOrDefaultAsync(t => t.SearchQuery == messageText, cancellationToken);

        if (cachedTrack != null)
        {
            logger.LogInformation("ЗНАЙШОВ! Отправляем file_id: {FileId}", cachedTrack.TelegramFileId);
            await botClient.SendAudio(
                chatId,
                InputFile.FromFileId(cachedTrack.TelegramFileId),
                cancellationToken: cancellationToken
            );
            return;
        }


        var userQuality = "high"; // по базе оставлю высокое. пока что
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.ChatId == chatId, cancellationToken);

        if (user != null) userQuality = user.QualityPreference;

        await botClient.SendMessage(chatId, "⏳ Ищу и скачиваю трек...", cancellationToken: cancellationToken);

        _ = Task.Run(async () =>
        {
            string? filePath = null; 

            try
            {
                filePath = await audioDownloaderService.DownloadAudioAsync(messageText, userQuality,
                    CancellationToken.None);

                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > 50 * 1024 * 1024)
                {
                    await botClient.SendMessage(chatId, "❌ Трек весит больше 50МБ.",
                        cancellationToken: CancellationToken.None);
                    return;
                }

                await using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var inputFile = InputFile.FromStream(stream, $"{messageText}.mp3");

                    var sentMessage = await botClient.SendAudio(
                        chatId,
                        inputFile,
                        cancellationToken: CancellationToken.None);

                    if (sentMessage.Audio != null)
                    {
                        using var saveScope = scopeFactory.CreateScope();
                        var saveDbContext = saveScope.ServiceProvider.GetRequiredService<ServiceDbContext>();

                        saveDbContext.TrackCaches.Add(new TrackReference
                        {
                            SearchQuery = messageText,
                            TelegramFileId = sentMessage.Audio.FileId
                        });
                        await saveDbContext.SaveChangesAsync(CancellationToken.None);
                        logger.LogInformation("Трек сохранен в БД.");
                    }
                } // Использование using (...) для потока закрывает файл сразу после отправки. До этой дичи дошёл 12.06.26 в 23:19:52(два раза эту дрисню переписывал)
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Ошибка при загрузке файла");
                await botClient.SendMessage(chatId, $"❌ Ошибка: {exception.Message}",
                    cancellationToken: CancellationToken.None);
            }
            finally
            {
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    File.Delete(filePath);
                    logger.LogInformation("Удален временный файл: {FilePath}", filePath);
                }
            }
        }, 
            cancellationToken);
    }
}