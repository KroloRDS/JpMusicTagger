using JpMusicTagger.Utils;

namespace JpMusicTagger.VGMDB;

public static class SearchResultsParser
{
	public static string? GetAlbumId(string html, string albumName)
	{
		var table = html.Cut("<tbody>", "</tbody>") ?? string.Empty;
		var rows = table.Split("</tr>");
		var matchingRows = rows.Where(x => MatchAlbumName(x, albumName));

		return matchingRows.Count() != 1 ? null :
			GetAlbumId(matchingRows.First());
	}

	private static bool MatchAlbumName(string html, string albumName)
	{
		var nameFromHtml = GetAlbumName(html);
		if (nameFromHtml is null) return false;
		return nameFromHtml.ToLower() == albumName.ToLower();
	}

	private static string? GetAlbumName(string html)
	{
		var cut = html.Cut("class=\"albumtitle\"");
		if (cut is null) return null;

		cut = cut.Cut(null, "</span>");
		if (cut is null) return null;

		var index = cut.LastIndexOf('>');
		if (index == -1) return null;

		var name = cut[(index + 1)..];
		return name.Trim();
	}

	private static string? GetAlbumId(string html)
	{
		var cut = html.Cut("href=\"https://vgmdb.net/album/");
		if (cut is null) return null;

		cut = cut.Cut(null, "\"");
		return cut?.Trim();
	}
}
