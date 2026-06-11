using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBotYtMusic.Services;

public interface IUpdateHandlerService
{
    Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
    Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken);
}