using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBotYtMusic.Interfaces;

public interface ICallbackQueryService
{
    Task ProcessAsync(ITelegramBotClient botClient, CallbackQuery query, CancellationToken cancellationToken);
}