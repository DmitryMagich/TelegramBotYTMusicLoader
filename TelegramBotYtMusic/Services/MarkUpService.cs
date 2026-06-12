using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotYtMusic.Services;

public static class MarkUpService
{
    public static InlineKeyboardMarkup GetQualityKeyboard()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[] 
            { 
                InlineKeyboardButton.WithCallbackData("⬇️ 320kbps", "set_high"), 
                InlineKeyboardButton.WithCallbackData("⬇️ 128kbps", "set_low") 
            }
        });
    }
    
    
}