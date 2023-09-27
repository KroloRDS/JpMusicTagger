using JpMusicTagger.Config;
using JpMusicTagger.Logging;
using System.Text;
using System.Text.Json;

namespace JpMusicTagger.Google;

public static class GoogleApi
{
	private static readonly string TranslateUrl = "https://translate.googleapis.com/translate_a/single";
	private static readonly string RomaniseUrl = "https://translation.googleapis.com/v3/projects/";
	private static readonly string TokenUrl = "https://oauth2.googleapis.com/token";

	private static readonly HttpClient _generalClient = new();
	private static HttpClient? _romaniseClient = null;

	public static async Task Init(string? credentialsJsonPath)
	{
		var credentials = GetCredentials(credentialsJsonPath);
		var token = await GetAccessToken(credentials);
		var projectId = credentials.ProjectId.ToLower();

		_romaniseClient = new HttpClient();
		_romaniseClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
		_romaniseClient.DefaultRequestHeaders.Add("x-goog-user-project", projectId);
		_romaniseClient.BaseAddress = new Uri(RomaniseUrl + projectId + ":romanizeText");
		GlobalConfig.UseGoogleRomanisation = true;
	}

	private static Credentials GetCredentials(string? credentialsJsonPath)
	{
		if (!File.Exists(credentialsJsonPath))
			throw new Exception($"File {credentialsJsonPath} does not exist");

		var json = File.ReadAllText(credentialsJsonPath);
		var credentials = JsonSerializer.Deserialize<Credentials>(json);

		return credentials ??
			throw new Exception("Error during serialization of Google credentials json");
	}

	private static async Task<string> GetAccessToken(Credentials credentials)
	{
		credentials.GrantType ??= "refresh_token";
		var json = JsonSerializer.Serialize(credentials);
		var content = new StringContent(json,
			Encoding.UTF8, "application/json");

		var response = await _generalClient.PostAsync(TokenUrl, content);
		if (!response.IsSuccessStatusCode)
			throw new Exception($"Google returned error code {response.StatusCode}");

		var result = await response.Content.ReadAsStringAsync();
		var deserialised = JsonSerializer.Deserialize<TokenResponse>(result);
		var token = deserialised?.AccessToken;
		if (string.IsNullOrWhiteSpace(token))
			throw new Exception("Google returned empty token");

		return token;
	}

	public static async Task<string> Translate(string text)
	{
		var query = "?client=gtx&sl=ja&tl=en&dt=t&q=" + text;
		var response = await _generalClient.GetAsync(TranslateUrl + query);
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
		if (_romaniseClient is null)
		{
			await Logger.Log("Google API client was not properly initialised");
			return text;
		}

		var request = BuildRominseTextRequest(text);
		var response = await _romaniseClient.PostAsync("", request);
		if (!response.IsSuccessStatusCode)
		{
			await Logger.Log($"Google romanisation API returned error code {response.StatusCode}");
			return text;
		}

		var json = await response.Content.ReadAsStringAsync();
		try
		{
			var result = JsonSerializer.Deserialize<RomanisationResponse>(json);
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
			contents = new string[] { text },
			sourceLanguageCode = "ja"
		});
		var content = new StringContent(json, Encoding.UTF8, "application/json");
		return content;
	}
}
