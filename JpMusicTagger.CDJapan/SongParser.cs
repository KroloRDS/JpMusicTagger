using JpMusicTagger.Tags;
using JpMusicTagger.Extensions;

namespace JpMusicTagger.CDJapan;

public class SongParser
{
	public static SongTags GetSongTags(string html,
		AlbumTags album, int? diskNumber)
	{
		var song = new SongTags
		{
			Album = album,
			DiscNumber = diskNumber,
			Title = GetSongTitle(html),
			TrackNumber = GetTrackNumber(html)
		};
		return song;
	}

	private static string GetSongTitle(string html)
	{
		var cut = html.Cut("<div style=\"font-size:0.8em;\">");
		if (cut is null) return string.Empty;

		cut = cut.Cut(null, "<");
		if (cut is null) return string.Empty;

		cut = cut?.Trim().FixHtmlSpecialChars();
		return cut ?? string.Empty;
	}

	private static int? GetTrackNumber(string html)
	{
		var cut = html.Cut("<td class=\"track-no\">");
		if (cut is null) return null;

		cut = cut.Cut(null, "<");
		if (cut is null) return null;

		var success = int.TryParse(cut, out var trackNumber);
		return success ? trackNumber : null;
	}
}
