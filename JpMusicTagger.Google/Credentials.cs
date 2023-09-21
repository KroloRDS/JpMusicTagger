using System.Text.Json.Serialization;

namespace JpMusicTagger.Google;

public class Credentials
{
	[JsonPropertyName("client_id")]
	public string ClientId { get; set; } = string.Empty;

	[JsonPropertyName("client_secret")]
	public string ClientSecret { get; set; } = string.Empty;

	[JsonPropertyName("refresh_token")]
	public string RefreshToken { get; set; } = string.Empty;

	[JsonPropertyName("quota_project_id")]
	public string ProjectId { get; set; } = string.Empty;

	[JsonPropertyName("type")]
	public string Type { get; set; } = string.Empty;

	[JsonPropertyName("grant_type")]
	public string? GrantType { get; set; }
}
