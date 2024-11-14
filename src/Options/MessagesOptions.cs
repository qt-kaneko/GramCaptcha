namespace GramCaptcha.Options;

class MessagesOptions
{
  public required string BeginMessageText { get; init; }
  public required string EndMessageText { get; init; }
  public required string SolveButtonText { get; init; }
}