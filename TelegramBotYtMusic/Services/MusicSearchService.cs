using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotYtMusic.Data;
using TelegramBotYtMusic.Entities;

namespace TelegramBotYtMusic.Services;

public class MusicSearchService(
    ILogger<MusicSearchService> logger,
    IAudioDownloaderService audioDownloaderService,
    IServiceScopeFactory scopeFactory) : IMusicSearchService
{
    public async Task ProcessAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
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
                chatId: chatId,
                audio: InputFile.FromFileId(cachedTrack.TelegramFileId),
                cancellationToken: cancellationToken
            );
            return; 
        }
        
        await botClient.SendMessage(chatId, "⏳ Ищу и скачиваю трек, подожди немного...", cancellationToken: cancellationToken);

        _ = Task.Run(async () =>
        {
            try
            {
                var filePath = await audioDownloaderService.DownloadAudioAsync(messageText, CancellationToken.None);
                
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > 50 * 1024 * 1024) 
                {
                    await botClient.SendMessage(chatId, "❌ Трек весит больше 50МБ.", cancellationToken: CancellationToken.None);
                    if (File.Exists(filePath)) File.Delete(filePath);
                    return;
                }

                await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var inputFile = InputFile.FromStream(stream, Path.GetFileName(filePath));
                
                var sentMessage = await botClient.SendAudio(
                    chatId: chatId, 
                    audio: inputFile, 
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
                
                if (File.Exists(filePath)) File.Delete(filePath);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Ошибка при загрузке файла");
                await botClient.SendMessage(chatId, $"❌ Ошибка: {exception.Message}", cancellationToken: CancellationToken.None);
            }
        }, cancellationToken);
    }
}