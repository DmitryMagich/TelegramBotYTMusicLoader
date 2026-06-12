using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotYtMusic.Database;
using TelegramBotYtMusic.Entities;
using TelegramBotYtMusic.Interfaces;

namespace TelegramBotYtMusic.Services;

public class CallbackQueryService(
    ILogger<CallbackQueryService> logger,
    IServiceScopeFactory scopeFactory) : ICallbackQueryService
{
    public async Task ProcessAsync(ITelegramBotClient botClient, CallbackQuery query, CancellationToken cancellationToken)
    {
        if (query.Data == null || query.Message == null) return;

        var chatId = query.Message.Chat.Id;
        if (query.Data.StartsWith("set_"))
        {
            var quality = query.Data == "set_high" ? "high" : "low";
            var textStatus = quality == "high" ? "320kbps" : "128kbps";
            
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.ChatId == chatId, cancellationToken);
            
            if (user == null)
            {
                user = new AppUser { ChatId = chatId, QualityPreference = quality };
                dbContext.Users.Add(user);
            }
            else
            {
                user.QualityPreference = quality;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await botClient.AnswerCallbackQuery(query.Id, $"Сохранено: {textStatus}");
            await botClient.EditMessageText(
                chatId: chatId,
                messageId: query.Message.MessageId,
                text: $"✅ Текущее качество установлено на: **{textStatus}**.\nТеперь просто отправь название песни!",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
        }
    }
}