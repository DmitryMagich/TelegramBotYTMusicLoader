using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBotYtMusic.Services;

public class CallbackQueryService(ILogger<ICallbackQueryService> logger) : ICallbackQueryService
{
    public async Task ProcessAsync(ITelegramBotClient botClient, CallbackQuery query, CancellationToken ct)
    {
        await botClient.AnswerCallbackQuery(query.Id, "Лоадинг...");

        switch (query.Data)
        {
            case "dl_high":
                await botClient.SendMessage(query.Message!.Chat.Id, "Начинаю загрузку в 320kbps...");
                break;
                
            case "dl_low":
                await botClient.SendMessage(query.Message!.Chat.Id, "Начинаю загрузку в 128kbps...");
                break;
                
            default:
                logger.LogWarning("Получен неизвестный каллбэк: {Data}", query.Data);
                break;
        }
    }
}