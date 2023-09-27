using JpMusicTagger.Extensions;
using JpMusicTagger.Tags;

namespace JpMusicTagger.VGMDB;

public class SongParser
{
	public static SongTags GetSongTags(string html,
		AlbumTags album, int? diskNumber)
	{
		var song = new SongTags
		{
			Album = album,
			DiscNumber = diskNumber,
			Length = GetSongLength(html),
			Title = GetSongTitle(html),
			TrackNumber = GetTrackNumber(html)
		};
		return song;
	}

	private static TimeSpan GetSongLength(string html)
	{
		var cut = html.Cut("<span class=\"time\">");
		if (cut is null) return TimeSpan.Zero;

		cut = cut.Cut(null, "<");
		if (cut is null) return TimeSpan.Zero;

		cut = cut.Trim();
		var split = cut.Split(':');
		if (split.Length != 2) return TimeSpan.Zero;

		var seconds = 0;
		var success = int.TryParse(split[0], out var minutes)
			&& int.TryParse(split[1], out seconds);
		return success ? new TimeSpan(0, minutes, seconds) : TimeSpan.Zero;
	}

	private static string GetSongTitle(string html)
	{
		var cut = html.Cut("colspan=\"2\">");
		if (cut is null) return string.Empty;

		cut = cut.Cut(null, "<");
		if (cut is null) return string.Empty;

		cut = cut?.Trim().FixHtmlSpecialChars();
		return cut ?? string.Empty;
	}

	private static int? GetTrackNumber(string html)
	{
		var cut = html.Cut("<span class=\"label\">");
		if (cut is null) return null;

		cut = cut.Cut(null, "<");
		if (cut is null) return null;

		if (cut[0] == '0') cut = cut[1..];

		var success = int.TryParse(cut, out var trackNumber);
		return success ? trackNumber : null;
	}
}
