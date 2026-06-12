using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBotYtMusic.Interfaces;

public interface IMusicSearchService
{
    Task ProcessAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken);
}