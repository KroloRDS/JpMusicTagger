namespace JpMusicTagger.Utils;

public static class StringExtensions
{
	public static string? Cut(this string text,
		string? start, string? end = null)
	{
		var startIndex = string.IsNullOrWhiteSpace(start) ?
			0 : text.IndexOf(start);
		if (startIndex == -1) return null;
		startIndex += start?.Length ?? 0;

		var endIndex = string.IsNullOrWhiteSpace(end) ?
			text.Length : text.IndexOf(end);
		if (endIndex == -1) return null;

		var result = text[startIndex..endIndex];
		return result;
	}
}
