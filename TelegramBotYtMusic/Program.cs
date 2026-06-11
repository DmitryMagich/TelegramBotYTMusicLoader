using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using TelegramBotYtMusic;
using TelegramBotYtMusic.Data;
using TelegramBotYtMusic.Services;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ServiceDbContext>(options =>
    options.UseNpgsql(connectionString));

var botToken = builder.Configuration["BotConfiguration:BotToken"];
builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));
builder.Services.AddTransient<IUpdateHandlerService, UpdateHandlerService>();
builder.Services.AddTransient<IAudioDownloaderService, AudioDownloaderService>();
builder.Services.AddTransient<ICommandService, CommandService>();
builder.Services.AddTransient<IMusicSearchService, MusicSearchService>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();