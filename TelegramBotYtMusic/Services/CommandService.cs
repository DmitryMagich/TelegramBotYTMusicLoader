using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBotYtMusic.Services;
// ес чё, 02:01 ночи, я сижу строчу эту хрень, help me.
public class CommandService(ILogger<ICommandService> logger) : ICommandService
{
    public async Task ProcessAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var command = message.Text?.Split(' ')[0].ToLower();

        switch (command)
        {
            case "/start":
                await botClient.SendMessage(message.Chat.Id, 
                    "Вечер в хату молодым! Я твой личный(гы) музыкальный бот. \n" +
                    "Просто напиши название песни или исполнителя(не стоит, я могу сломаться и выдать тебе фильм на 2 часа и убить сервер(точнее добить его ссд)), и я найду её для тебя!", 
                    cancellationToken: cancellationToken);
                break;

            case "/help":
                await botClient.SendMessage(message.Chat.Id, 
                    "Доступные команды:\n" +
                    "/start - Запуск бота\n" +
                    "/help - Помощь [ нету:) ]", 
                    cancellationToken: cancellationToken);
                break;
                
            case "/ping":
                await botClient.SendMessage(
                    message.Chat.Id,
                    "pong",
                    cancellationToken: cancellationToken);
                break;
            
            default:
                await botClient.SendMessage(message.Chat.Id, "Неизвестная команда.", cancellationToken: cancellationToken);
                break;
        }
    }
}