using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBotYtMusic.Services;

public interface ICallbackQueryService
{
    Task ProcessAsync(ITelegramBotClient botClient, CallbackQuery query, CancellationToken cancellationToken);
}