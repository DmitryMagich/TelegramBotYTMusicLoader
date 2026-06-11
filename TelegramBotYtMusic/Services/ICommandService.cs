using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBotYtMusic.Services;

public interface ICommandService
{
    Task ProcessAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken);
}