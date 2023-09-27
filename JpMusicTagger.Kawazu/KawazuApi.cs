using Kawazu;
using JpMusicTagger.Config;
using JpMusicTagger.Logging;

namespace JpMusicTagger.Kawazu;

public static class KawazuApi
{
	private static KawazuConverter? _converter = null;

	public static void Init(string? dictionaryPath = null)
	{
		_converter = new KawazuConverter(dictionaryPath);
		GlobalConfig.UseKawazuRomanisation = true;
	}

	public static async Task<string> Romanise(string text)
	{
		if (_converter is null)
		{
			await Logger.Log("Kawazu client was not properly initialised");
			return text;
		}

		var result = await _converter.Convert(
			text, To.Romaji, Mode.Spaced, RomajiSystem.Hepburn);
		return result.Trim();
	}
}
