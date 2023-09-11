using JpMusicTagger.Logging;
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
	
	private static readonly HttpClient _translateClient = InitTranslateClient();
	private static readonly HttpClient _romaniseClient = InitRomiseClient();

	private static HttpClient InitTranslateClient()
	{
		var client = new HttpClient
		{
			BaseAddress = new Uri(TranslateUrl)
		};
		return client;
	}

	private static HttpClient InitRomiseClient()
	{
		var client = new HttpClient();
		var credentials = GetGoogleCredentials();
		if (credentials is null) return client;

		client.DefaultRequestHeaders.Add("Authorization", $"Bearer {credentials.Token}");
		client.DefaultRequestHeaders.Add("x-goog-user-project", credentials.ProjectId);
		client.BaseAddress = new Uri(RomaniseUrl + credentials.ProjectId + ":romanizeText");

		return client;
	}

	private static ApiCredentials? GetGoogleCredentials()
	{
		var path = Environment.GetEnvironmentVariable("GOOGLE_API_CREDENTIALS");
		if (path is null)
		{
			Logger.Log("Env variable GOOGLE_API_CREDENTIALS couldn't be found").Wait();
			return null;
		};

		var text = File.ReadAllText(path);
		try
		{
			var credentials = JsonSerializer.Deserialize<ApiCredentials>(text);
			return credentials;
		}
		catch (Exception ex)
		{
			var msg = "GOOGLE_API_CREDENTIALS couldn't be deserialised. " + ex.Message;
			Logger.Log(msg).Wait();
			return null;
		}
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

	private class ApiCredentials
	{
		public string ProjectId { get; set; } = string.Empty;
		public string Token { get; set; } = string.Empty;
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
