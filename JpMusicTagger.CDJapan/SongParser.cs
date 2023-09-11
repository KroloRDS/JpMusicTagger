using JpMusicTagger.Tags;
using JpMusicTagger.Utils;

namespace JpMusicTagger.CDJapan;

public class SongParser
{
	public static SongTags GetSongTags(string html,
		AlbumTags album, uint diskNumber)
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

	private static uint GetTrackNumber(string html)
	{
		var cut = html.Cut("<td class=\"track-no\">");
		if (cut is null) return 0U;

		cut = cut.Cut(null, "<");
		if (cut is null) return 0U;

		var success = uint.TryParse(cut, out var trackNumber);
		return success ? trackNumber : 0U;
	}
}
