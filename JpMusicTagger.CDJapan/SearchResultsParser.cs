using JpMusicTagger.Extensions;

namespace JpMusicTagger.CDJapan;

public class SearchResultsParser
{
	public static string? GetCatalogNumber(string html, string albumName)
	{
		var items = html.Split("<li class=\"item\">");
		if (items.Length > 1) items = items.Skip(1).ToArray();

		if (items.Length > 1)
			items = items.Where(x => MatchAlbumName(x, albumName)).ToArray();

		return items.Length != 1 ? null : GetCatalogNumer(items.First());
	}

	private static bool MatchAlbumName(string html, string albumName)
	{
		var nameFromHtml = GetAlbumName(html);
		if (nameFromHtml is null) return false;
		return nameFromHtml.ToLower() == albumName.ToLower();
	}

	private static string? GetAlbumName(string item)
	{
		var cut = item.Cut("<span class=\"title-text\" itemprop=\"name\">");
		if (cut is null) return null;

		var name = cut.Cut(null, "</span>");
		return name?.Trim();
	}

	private static string? GetCatalogNumer(string searchRow)
	{
		var cut = searchRow.Cut("href=\"https://www.cdjapan.co.jp/product/");
		if (cut is null) return null;

		cut = cut.Cut(null, "\"");
		return cut?.Trim();
	}
}
