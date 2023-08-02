using JpMusicTagger.Logging;

namespace JpMusicTagger.Romanising;

public static class GoogleTranslate
{
	private static readonly HttpClient _client = new();
	private static readonly string BaseUrl = "https://translate.googleapis.com/translate_a/single?client=gtx&sl=ja&tl=en&dt=t&q=";

	public static async Task<string> Translate(string text)
	{
		var uri = new Uri(BaseUrl + text);
		var response = await _client.GetAsync(uri);
		var result = await response.Content.ReadAsStringAsync();
		var index = result?.IndexOf(',') - 1 ?? -1;

		if (index == -1 || result!.StartsWith("<html"))
		{
			await Logger.Log($"Error during GoogleTranslate of {text}");
			return text;
		}

		return result[4..index];
	}
}
