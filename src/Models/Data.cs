using System.Text.Json.Serialization;

namespace GramCaptcha.Models;

struct Data
{
  [JsonPropertyName("chat_id")]
  public required long ChatId { get; init; }

  [JsonPropertyName("token")]
  public required string Token { get; init; }
}