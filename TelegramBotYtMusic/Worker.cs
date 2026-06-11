using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using TelegramBotYtMusic.Services;

namespace TelegramBotYtMusic;

public class Worker(
    ILogger<Worker> logger, 
    ITelegramBotClient botClient, 
    IUpdateHandlerService updateHandlerService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() 
        };

        logger.LogInformation("Запуск бота (Clean Architecture edition)...");

        botClient.StartReceiving(
            updateHandler: updateHandlerService.HandleUpdateAsync,
            errorHandler: updateHandlerService.HandleErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken
        );

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}