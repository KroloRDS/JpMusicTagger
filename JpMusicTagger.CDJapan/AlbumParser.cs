﻿using JpMusicTagger.Tags;
using JpMusicTagger.Utils;

namespace JpMusicTagger.CDJapan;

public class AlbumParser
{
	public static IEnumerable<SongTags> Parse(string html, string catalogNumer)
	{
		var songs = new List<SongTags>();
		var album = GetAlbumTags(html);
		album.CatalogNumber = catalogNumer;

		var disks = GetDisks(html);
		var diskNumber = disks.Count() > 1 ? 1U : 0U;

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
		return cut ?? string.Empty;
	}

	private static uint GetReleaseYear(string html)
	{
		var cut = html.Cut("<span itemprop=\"releaseDate\">");
		if (cut is null) return 0U;

		cut = cut.Cut(null, "</span>");
		if (cut is null || cut.Length < 4) return 0U;
		cut = cut[^4..];

		var valid = uint.TryParse(cut, out var year);
		if (valid) valid = year > 1000 && year < 2050;
		return valid ? year : 0U;
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
