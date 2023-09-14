using JpMusicTagger.Logging;
using JpMusicTagger.Utils;
using System.Text.Json;
using System.Text;

namespace JpMusicTagger.Google;

public static class GoogleApi
{
	private static readonly string TranslateUrl = "https://translate.googleapis.com/translate_a/single";
	private static readonly string RomaniseUrl = "https://translation.googleapis.com/v3/projects/";

	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};
	
	private static readonly HttpClient _translateClient;
	private static readonly HttpClient _romaniseClient;

	static GoogleApi()
	{
		_translateClient = InitTranslateClient();

		var consoleArgs = Environment.GetCommandLineArgs();

		var token = CliHelper.GetParameter("-t", consoleArgs) ??
			CliHelper.GetParameter("-token", consoleArgs);
		if (token is null)
		{
			Logger.Log("Missing parameter: -token").Wait();
			throw new ArgumentNullException("Token");
		}

		var projectId = CliHelper.GetParameter("-i", consoleArgs) ??
			CliHelper.GetParameter("-id", consoleArgs);
		if (projectId is null)
		{
			Logger.Log("Missing parameter: -id").Wait();
			throw new ArgumentNullException("ProjectID");
		}
		
		_romaniseClient = InitRomiseClient(token, projectId);
	}

	private static HttpClient InitTranslateClient()
	{
		var client = new HttpClient
		{
			BaseAddress = new Uri(TranslateUrl)
		};
		return client;
	}

	private static HttpClient InitRomiseClient
		(string? token, string? projectId)
	{
		var client = new HttpClient();
		if (string.IsNullOrWhiteSpace(token) ||
			string.IsNullOrWhiteSpace(projectId)) return client;

		client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
		client.DefaultRequestHeaders.Add("x-goog-user-project", projectId);
		client.BaseAddress = new Uri(RomaniseUrl + projectId + ":romanizeText");

		return client;
	}

	public static async Task<string> Translate(string text)
	{
		var query = "?client=gtx&sl=ja&tl=en&dt=t&q=" + text;
		var response = await _translateClient.GetAsync(query);
		var result = await response.Content.ReadAsStringAsync();
		var index = result?.IndexOf(',') - 1 ?? -1;

		if (index == -1 || result!.StartsWith("<html"))
		{
			await Logger.Log($"Error during GoogleTranslate of {text}");
			return text;
		}

		return result[4..index];
	}

	public static async Task<string> Romanise(string text)
	{
		var request = BuildRominseTextRequest(text);
		try
		{
			var response = await _romaniseClient.PostAsync("", request);
			if (!response.IsSuccessStatusCode)
				throw new Exception($"Google returned error code {response.StatusCode}");

			var json = await response.Content.ReadAsStringAsync();
			var result = JsonSerializer.Deserialize<RomanisationResponse>(
				json, JsonOptions);

			var romanised = result?.Romanizations[0]?.RomanizedText ??
				throw new Exception("Google response did not follow expected format");
			
			return romanised;
		}
		catch (Exception ex)
		{
			await Logger.Log($"Error during Romanisation of {text}: {ex.Message}");
			return text;
		}
	}

	private static HttpContent BuildRominseTextRequest(string text)
	{
		var json = JsonSerializer.Serialize(new
		{
			Contents = new string[] { text },
			SourceLanguageCode = "ja"
		}, JsonOptions);
		var content = new StringContent(json, Encoding.UTF8, "application/json");
		return content;
	}

	private class RomanisationResponse
	{
		public RomanisationResponseEntry[] Romanizations { get; set; } =
			Array.Empty<RomanisationResponseEntry>();
	}

	private class RomanisationResponseEntry
	{
		public string RomanizedText { get; set; } = string.Empty;
	}
}
