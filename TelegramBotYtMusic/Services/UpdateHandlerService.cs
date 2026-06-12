using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using TelegramBotYtMusic.Interfaces;


namespace TelegramBotYtMusic.Services;

public class UpdateHandlerService(
    ILogger<UpdateHandlerService> logger, 
    ICommandService commandService, 
    IMusicSearchService musicSearchService,
    ICallbackQueryService callbackQueryService) : IUpdateHandlerService
{
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.CallbackQuery is { } callbackQuery)
        {
            await callbackQueryService.ProcessAsync(botClient, callbackQuery, cancellationToken);
            return;
        }
        
        if (update.Message is not { } message) return;

        if (message.Text?.StartsWith('/') == true)
        {
            await commandService.ProcessAsync(botClient, message, cancellationToken);
        }
        else if (message.Text != null)
        {
            await musicSearchService.MusicSearchAsync(botClient, message, cancellationToken);
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