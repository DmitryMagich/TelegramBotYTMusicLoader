using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBotYtMusic.Services;

public interface IMusicSearchService
{
    Task ProcessAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken);
}