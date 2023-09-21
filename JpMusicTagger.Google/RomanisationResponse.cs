using System.Text.Json.Serialization;

namespace JpMusicTagger.Google;

public class RomanisationResponse
{
	[JsonPropertyName("romanizations")]
	public RomanisationResponseEntry[] Romanizations { get; set; } =
			Array.Empty<RomanisationResponseEntry>();
}

public class RomanisationResponseEntry
{
	[JsonPropertyName("romanizedText")]
	public string RomanizedText { get; set; } = string.Empty;
}
