using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using TelegramBotYtMusic.Data;
using TelegramBotYtMusic.Entities;

namespace TelegramBotYtMusic.Services;

public class UpdateHandlerService(ILogger<UpdateHandlerService> logger,
    IAudioDownloaderService audioDownloaderService,
    IServiceScopeFactory scopeFactory) : IUpdateHandlerService
{
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message) return;
        if (message.Text is not { } messageText) return;
        
        var chatId = message.Chat.Id;
        logger.LogInformation("Запрос: {MessageText}", messageText);
        
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
        
        var cachedTrack = await dbContext.TrackCaches
            .FirstOrDefaultAsync(t => t.SearchQuery == messageText, cancellationToken);

        if (cachedTrack != null)
        {
            logger.LogInformation("ЗНАЙШОВ ЗНАЙШОВ! Посылаю file_id: {FileId}", cachedTrack.TelegramFileId);
            await botClient.SendAudio(
                chatId: chatId,
                audio: InputFile.FromFileId(cachedTrack.TelegramFileId),
                cancellationToken: cancellationToken
            );
            return; 
        }
        
        await botClient.SendMessage(chatId, "⏳ Ищу и скачиваю трек. Вы первый кто запросили данный трек, потому подождите немного :L", cancellationToken: cancellationToken);

        _ = Task.Run(async () =>
        {
            try
            {
                var filePath = await audioDownloaderService.DownloadAudioAsync(messageText, CancellationToken.None);
                
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > 50 * 1024 * 1024) 
                {
                    await botClient.SendMessage(chatId, "❌ Трек весит больше 50МБ. Telegram не пропустит.", cancellationToken: CancellationToken.None);
                    if (File.Exists(filePath)) File.Delete(filePath);
                    return;
                }

                try 
                {
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
                        
                        var newCacheEntity = new TrackReference()
                        {
                            SearchQuery = messageText,
                            TelegramFileId = sentMessage.Audio.FileId
                        };
                        
                        saveDbContext.TrackCaches.Add(newCacheEntity);
                        await saveDbContext.SaveChangesAsync(CancellationToken.None);
                        
                        logger.LogInformation("Трек сохранен в бдшку с file_id: {FileId}", newCacheEntity.TelegramFileId);
                    }
                }
                finally
                {
                    if (File.Exists(filePath)) File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при загрузке");
                await botClient.SendMessage(chatId, $"❌ Ошибка: {ex.Message}", cancellationToken: CancellationToken.None);
            }
        });
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Ошибка Telegram API:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        logger.LogError(errorMessage);
        return Task.CompletedTask;
    }
}