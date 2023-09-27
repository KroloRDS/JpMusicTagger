using JpMusicTagger.Extensions;
using JpMusicTagger.Tags;

namespace JpMusicTagger.VGMDB;

public static class AlbumParser
{
	public static IEnumerable<SongTags> Parse(string html)
	{
		var songs = new List<SongTags>();
		var album = GetAlbumTags(html);

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
			CatalogNumber = GetCatalogNumber(html),
			Name = GetAlbumName(html),
			Composer = GetComposer(html),
			Year = GetReleaseYear(html)
		};
		return album;
	}

	private static string GetAlbumArtist(string html)
	{
		var cut = html.Cut("Publisher");
		if (cut is null) return string.Empty;

		cut = cut.Cut("<span");
		if (cut is null) return string.Empty;

		cut = cut.Cut(">", "<");
		cut = cut?.Trim().FixHtmlSpecialChars();
		return cut ?? string.Empty;
	}

	private static string GetCatalogNumber(string html)
	{
		var cut = html.Cut("Catalog Number");
		if (cut is null) return string.Empty;

		cut = cut.Cut("<td");
		if (cut is null) return string.Empty;

		cut = cut.Cut(">", "<");
		return cut?.Trim() ?? string.Empty;
	}

	private static string GetAlbumName(string html)
	{
		var cut = html.Cut("<h1><span class=\"albumtitle\"");
		if (cut is null) return string.Empty;

		cut = cut.Cut(">", "<");
		cut = cut?.Trim().FixHtmlSpecialChars();
		if (cut is null) return string.Empty;

		var name = cut.ToTitleCase();
		return name;
	}

	private static string GetComposer(string html)
	{
		var cut = html.Cut("Compos");
		if (cut is null) return string.Empty;

		cut = cut.Cut("<a");
		if (cut is null) return string.Empty;

		cut = cut.Cut(">", "<");
		cut = cut?.Trim().FixHtmlSpecialChars();
		return cut ?? string.Empty;
	}

	private static int? GetReleaseYear(string html)
	{
		var cut = html.Cut("Catalog Number");
		if (cut is null) return null;

		cut = cut.Cut("<a");
		if (cut is null) return null;

		cut = cut.Cut(">", "<");
		if (cut is null || cut.Length < 4) return null;
		cut = cut[^4..];

		var valid = int.TryParse(cut, out var year);
		if (valid) valid = year > 1000 && year < 2050;
		return valid ? year : null;
	}

	private static IEnumerable<string> GetDisks(string html)
	{
		var disks = Array.Empty<string>();
		var tracklistMarker = "<!-- / tracklist tools menu -->";

		var cut = html.Cut(tracklistMarker);
		if (cut is null) return disks;

		cut = cut.Contains(tracklistMarker) ?
			cut.Cut(null, tracklistMarker) :
			cut.Cut(null, "<h3>Notes");
		if (cut is null) return disks;

		disks = cut.Split("<table");
		if (disks.Length > 1) disks = disks.Skip(1).ToArray();
		return disks;
	}

	private static IEnumerable<string> GetTracks(string html)
	{
		var tracks = html.Split("<tr");
		if (tracks.Length > 1) tracks = tracks.Skip(1).ToArray();
		return tracks;
	}
}
