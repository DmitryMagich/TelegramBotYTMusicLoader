using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace TelegramBotYtMusic.Services;

public class UpdateHandlerService(
    ILogger<UpdateHandlerService> logger,
    IAudioDownloaderService audioDownloaderService) : IUpdateHandlerService
{
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message) return;
        if (message.Text is not { } messageText) return;
        
        var chatId = message.Chat.Id;
        logger.LogInformation("Делаю запросик: {MessageText}", messageText);
        await botClient.SendMessage(
            chatId: chatId,
            text: "⏳ Сёрчинг, почекай тришки...",
            cancellationToken: cancellationToken
        );
        
        _ = Task.Run(async () =>
        {
            try
            {
                var filePath = await audioDownloaderService.DownloadAudioAsync(messageText, CancellationToken.None);
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > 50 * 1024 * 1024)  // я долго думал оставлять комент или нет. но после того как оно мне скачало 1гб какогот подкаста в час ночи я ебал цю хуйню, пришлось проверку доавблять перед загрузкой..
                {
                    logger.LogWarning("Файл слишком большой: {Size} байт.", fileInfo.Length);
                    await botClient.SendMessage(
                        chatId: chatId,
                        text: "❌ Этот трек весит больше 50 МБ. Telegram API нинини для ботов такие большие файлики (ай шалунишка).",
                        cancellationToken: CancellationToken.None
                    );
                    if (File.Exists(filePath)) File.Delete(filePath);
            
                    return;
                }
                try 
                {
                    await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    var inputFile = InputFile.FromStream(stream, Path.GetFileName(filePath));
            
                    await botClient.SendAudio(
                        chatId: chatId, 
                        audio: inputFile, 
                        cancellationToken: CancellationToken.None);
                }
                finally
                {
                    if (File.Exists(filePath)) 
                        File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при скачивании");
                await botClient.SendMessage(
                    chatId: chatId,
                    text: $"❌ Произошла ошибка: {ex.Message}",
                    cancellationToken: CancellationToken.None
                );
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