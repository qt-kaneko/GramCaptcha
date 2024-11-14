using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using Telegram.Bot;
using Telegram.Bot.Polling;

namespace GramCaptcha;

class Worker(
  ITelegramBotClient _bot,
  IUpdateHandler _updateHandler
)
: BackgroundService
{
  override protected async Task ExecuteAsync(CancellationToken ct)
  {
    await _bot.ReceiveAsync(
      _updateHandler,
      cancellationToken: ct
    );
  }
}