using JpMusicTagger.Extensions;
using JpMusicTagger.Tags;

namespace JpMusicTagger.CDJapan;

public class AlbumParser
{
	public static IEnumerable<SongTags> Parse(string html, string catalogNumer)
	{
		var songs = new List<SongTags>();
		var album = GetAlbumTags(html);
		album.CatalogNumber = catalogNumer;

		var disks = GetDisks(html);
		int? diskNumber = disks.Count() > 1 ? 1 : null;

		foreach (var disk in disks)
		{
			var tracksHtml = GetTracks(disk);
			var tracks = tracksHtml.Select(
				x => SongParser.GetSongTags(x, album, diskNumber));
			songs.AddRange(tracks);
			diskNumber++;
		}
		return songs;
	}

	private static AlbumTags GetAlbumTags(string html)
	{
		var album = new AlbumTags
		{
			Artist = GetAlbumArtist(html),
			Name = GetAlbumName(html),
			Year = GetReleaseYear(html)
		};
		return album;
	}

	private static string GetAlbumArtist(string html)
	{
		var cut = html.Cut("<h3 class=\"person\">", "</h3>");
		if (cut is null) return string.Empty;

		cut = cut.Cut("<a href=");
		if (cut is null) return string.Empty;

		cut = cut.Cut(">", "<");
		cut = cut?.Trim().FixHtmlSpecialChars();
		return cut ?? string.Empty;
	}

	private static string GetAlbumName(string html)
	{
		var cut = html.Cut(null, "</span>");
		cut = cut?.Trim().FixHtmlSpecialChars();
		if (cut is null) return string.Empty;

		var name = cut.ToTitleCase();

		var index = name.IndexOf("[");
		if (index == -1) index = name.IndexOf("(");

        if (index > 0) name = name[..index];

        return name.Trim();
	}

	private static int? GetReleaseYear(string html)
	{
		var cut = html.Cut("<span itemprop=\"releaseDate\">");
		if (cut is null) return null;

		cut = cut.Cut(null, "</span>");
		if (cut is null || cut.Length < 4) return null;
		cut = cut[^4..];

		var valid = int.TryParse(cut, out var year);
		if (valid) valid = year > 1000 && year < 2050;
		return valid ? year : null;
	}

	private static IEnumerable<string> GetDisks(string html)
	{
		var disks = html.Split("<table class=\"tracklist\">");
		if (disks.Length > 1) disks = disks.Skip(1).ToArray();
		return disks;
	}

	private static IEnumerable<string> GetTracks(string html)
	{
		var tracks = html.Split("<tr>");
		if (tracks.Length > 1) tracks = tracks.Skip(1).ToArray();
		return tracks;
	}
}
