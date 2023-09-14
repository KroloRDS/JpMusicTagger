using JpMusicTagger.Tags;
using JpMusicTagger.Utils;

namespace JpMusicTagger.CDJapan;

public class CdJapanScrapper
{
	private static readonly HttpClient _httpClient = GetHttpClient();

	private static HttpClient GetHttpClient()
	{
		var client = new HttpClient
		{
			BaseAddress = new Uri("https://www.cdjapan.co.jp/")
		};
		return client;
	}

	public static async Task<IEnumerable<SongTags>> GetTags(
		string albumName, string? artist = null)
	{
		var songs = Enumerable.Empty<SongTags>();

		var search = await Search(albumName, artist);
		if (search is null) return songs;

		var catalogNumer = SearchResultsParser.GetCatalogNumber(search, albumName);
		if (catalogNumer is null) return songs;

		var album = await GetAlbum(catalogNumer);
		if (album is null) return songs;

		songs = AlbumParser.Parse(album, catalogNumer);
		return songs;
	}

	private static async Task<string?> Search(
		string album, string? artist = null)
	{
		var baseQuery = "searchuni?opt.exclude_prx=on&term.media_format=cd&q=";
		var searchPhrase = album;
		if (!string.IsNullOrWhiteSpace(artist))
			searchPhrase = artist + " " + searchPhrase;

		var response = await _httpClient.GetAsync(baseQuery + searchPhrase);
		if (!response.IsSuccessStatusCode) return null;

		var html = await response.Content.ReadAsStringAsync();
		var content = html.Cut("<div class=\"row sdw\" id=\"search-result\">",
			"<ul class=\"pager\">");
		return content;
	}

	private static async Task<string?> GetAlbum(string catalogNumber)
	{
		var url = "product/" + catalogNumber;
		var response = await _httpClient.GetAsync(url);
		if (!response.IsSuccessStatusCode) return null;

		var html = await response.Content.ReadAsStringAsync();
		var content = html.Cut("<h1><span itemprop=\"name\">",
			"<div class=\"row sdw\" id=\"customer-review\">");

		return content;
	}
}
