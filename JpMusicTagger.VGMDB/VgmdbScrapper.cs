using JpMusicTagger.Extensions;
using JpMusicTagger.Tags;

namespace JpMusicTagger.VGMDB;

public static class VgmdbScrapper
{
	private static readonly HttpClient _httpClient = GetHttpClient();

	private static HttpClient GetHttpClient()
	{
		var client = new HttpClient
		{
			BaseAddress = new Uri("https://vgmdb.net/")
		};
		return client;
	}

	public static async Task<IEnumerable<SongTags>> GetTags(string album)
	{
		var songs = Enumerable.Empty<SongTags>();

		(var title, var content) = await GetHtmlTitleAndContent(
			$"search?q={album}&type=album");
		if (title is null || content is null) return songs;

		if (title.Contains("Search Results"))
		{
			var id = SearchResultsParser.GetAlbumId(content, album);
			if (id is null) return songs;

			(_, content) = await GetHtmlTitleAndContent("album/" + id);
			if (title is null || content is null) return songs;
		}

		songs = AlbumParser.Parse(content);
		return songs;
	}

	private static async Task<(string?, string?)> GetHtmlTitleAndContent(string url)
	{
		var response = await _httpClient.GetAsync(url);
		if (!response.IsSuccessStatusCode) return (null, null);
		var html = await response.Content.ReadAsStringAsync();
		var title = html.Cut("<title>", "</title>");
		var content = html.Cut("<!-- main page contents -->");
		return (title, content);
	}
}
