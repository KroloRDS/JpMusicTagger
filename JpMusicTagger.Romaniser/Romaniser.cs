using Kawazu;

namespace JpMusicTagger.Romaniser;

public static class Romaniser
{
	private static readonly KawazuConverter _converter = new();

	public static async Task<string> Convert(string input)
	{
		var output = await _converter.Convert(
			input, To.Romaji, Mode.Spaced, RomajiSystem.Nippon);
		return output;
	}
}
