using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace TelegramBotYtMusic.Services;

public class UpdateHandlerService(
    ILogger<UpdateHandlerService> logger, // Вернул логгер!
    ICommandService commandService, 
    IMusicSearchService musicSearchService) : IUpdateHandlerService
{
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        if (update.Message is not { } message) return;
        if (message.Text?.StartsWith('/') == true)
        {
            await commandService.ProcessAsync(botClient, message, ct);
        }
        else if (message.Text != null)
        {
            await musicSearchService.ProcessAsync(botClient, message, ct);
        }
    }
    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Ошибка Telegram API:\n[{apiRequestException.ErrorCode}]\n[{apiRequestException.Message}]",
            _ => exception.ToString()
        };

        logger.LogError(errorMessage);
        return Task.CompletedTask;
    }
}