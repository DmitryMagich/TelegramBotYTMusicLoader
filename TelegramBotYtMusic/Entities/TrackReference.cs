namespace TelegramBotYtMusic.Entities;

public class TrackReference
{
    public Guid Id { get; set; }
    public required string SearchQuery { get; set; }
    public required string TelegramFileId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}