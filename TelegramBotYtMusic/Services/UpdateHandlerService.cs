using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace TelegramBotYtMusic.Services;

public class UpdateHandlerService(ILogger<UpdateHandlerService> logger) : IUpdateHandlerService
{
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message) return;
        if (message.Text is not { } messageText) return;
        
        var chatId = message.Chat.Id;
        logger.LogInformation("HandleMessage: {MessageText}", messageText);


        await botClient.SendMessage(
            chatId: chatId,
            text: "Echo: " + messageText + " :)",
            cancellationToken: cancellationToken
        );
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