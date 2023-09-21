using System.Text.Json.Serialization;

namespace JpMusicTagger.Google;

public class TokenResponse
{
	[JsonPropertyName("access_token")]
	public string AccessToken { get; set; } = string.Empty;
}
