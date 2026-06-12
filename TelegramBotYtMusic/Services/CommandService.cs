using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotYtMusic.Interfaces;


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
                    "Драсте! Я твой личный музыкальный бот. \n" +
                    "Просто напиши название песни или исполнителя, и я найду её для тебя! (Коль альбом целый хочешь. в конец запроса добавляй [album, albums])", 
                    cancellationToken: cancellationToken);
                break;

            case "/help":
                await botClient.SendMessage(message.Chat.Id, 
                    "Доступные команды:\n" +
                    "/start - Запуск бота\n" +
                    "/help - Помощь [ нету:) ]\n" +
                    "Ниже ты можешь выбрать в каком качестве качать треки.", 
                    replyMarkup: MarkUpService.GetQualityKeyboard(),
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