using System.ComponentModel.DataAnnotations;

namespace TelegramBotYtMusic.Entities;

public class AppUser
{
    [Key]public long ChatId { get; set; }
    public string QualityPreference { get; set; } = "high"; 
}