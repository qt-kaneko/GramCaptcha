using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using GramCaptcha;
using GramCaptcha.Options;
using GramCaptcha.Services;

using Telegram.Bot;
using Telegram.Bot.Polling;

var builder = Host.CreateApplicationBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

configuration.AddUserSecrets<Program>();

services.Configure<BotOptions>(configuration.GetSection("Bot"));
services.Configure<MessagesOptions>(configuration.GetSection("Messages"));
services.Configure<CaptchaOptions>(configuration.GetSection("Captcha"));
services.AddSingleton(services =>
  new TelegramBotClientOptions(services.GetRequiredService<IOptions<BotOptions>>()
                                       .Value
                                       .Token)
);
services.AddHttpClient();
services.AddSingleton<ITelegramBotClient, TelegramBotClient>();
services.AddSingleton<IUpdateHandler, UpdateHandler>();
services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();