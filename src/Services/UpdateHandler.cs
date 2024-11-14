using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using GramCaptcha.Models;
using GramCaptcha.Options;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GramCaptcha.Services;

class UpdateHandler(
  IOptions<MessagesOptions> messages,
  IOptions<CaptchaOptions> captcha,

  ILogger<UpdateHandler> _logger,
  HttpClient _http
)
: IUpdateHandler
{
  readonly MessagesOptions _messages = messages.Value;
  readonly CaptchaOptions _captcha = captcha.Value;

  public async Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, HandleErrorSource source, CancellationToken ct)
  {
    _logger.LogError(exception, "Error occurred");
  }

  public Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
  {
    return update switch {
      { ChatJoinRequest: {} chatJoinRequest } => HandleChatJoinRequestAsync(bot, chatJoinRequest, ct),
      { Message: {} message } => HandleMessageAsync(bot, message, ct),
      _ => Task.CompletedTask
    };
  }

  async Task HandleChatJoinRequestAsync(ITelegramBotClient bot, ChatJoinRequest chatJoinRequest, CancellationToken ct)
  {
    var webApp = new WebAppInfo(_captcha.Url + $"?start_param={chatJoinRequest.Chat.Id}");
    var replyMarkup = new ReplyKeyboardMarkup() {
      Keyboard = [
        [KeyboardButton.WithWebApp(_messages.SolveButtonText, webApp)]
      ],
      ResizeKeyboard = true
    };

    await bot.SendMessage(
      chatJoinRequest.UserChatId,
      _messages.BeginMessageText,
      replyMarkup: replyMarkup,
      cancellationToken: ct
    );
  }

  async Task HandleMessageAsync(ITelegramBotClient bot, Message message, CancellationToken ct)
  {
    if (message.WebAppData is not {} webAppData) return;

    await bot.SendMessage(
      message.Chat.Id,
      _messages.EndMessageText,
      replyMarkup: new ReplyKeyboardRemove()
    );

    Data data;
    try { data = JsonSerializer.Deserialize<Data>(webAppData.Data); }
    catch (JsonException) { return; }

    var response = await _http.PostAsync("https://challenges.cloudflare.com/turnstile/v0/siteverify", new FormUrlEncodedContent(new Dictionary<string, string>() {
      { "secret", _captcha.Secret },
      { "response", data.Token }
    }));
    var responseContent = await response.Content.ReadFromJsonAsync<TurnstileResponse>();

    if (responseContent.Success)
    {
      await bot.ApproveChatJoinRequest(data.ChatId, message.Chat.Id, cancellationToken: ct);
    }
    else
    {
      await bot.DeclineChatJoinRequest(data.ChatId, message.Chat.Id, cancellationToken: ct);
    }
  }
}