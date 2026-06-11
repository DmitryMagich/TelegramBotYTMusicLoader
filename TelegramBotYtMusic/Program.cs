using Telegram.Bot;
using TelegramBotYtMusic;
using TelegramBotYtMusic.Services;

var builder = Host.CreateApplicationBuilder(args);

var botToken = builder.Configuration["BotConfiguration:BotToken"];
if (string.IsNullOrEmpty(botToken))
    throw new ArgumentNullException(nameof(botToken), "Токен бота не найден в appsettings.json!");
builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));
builder.Services.AddTransient<IUpdateHandlerService, UpdateHandlerService>();
builder.Services.AddTransient<IAudioDownloaderService, AudioDownloaderService>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();