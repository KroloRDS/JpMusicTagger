using System.Globalization;

namespace JpMusicTagger.Utils;

public static class StringExtensions
{
	private static readonly TextInfo TextInfo =
		new CultureInfo("en-US", false).TextInfo;

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

	public static string? FixHtmlSpecialChars(this string? text)
	{
		if (string.IsNullOrWhiteSpace(text)) return text;

		text = text.Replace("&quot;", "\"");
		text = text.Replace("&apos;", "'");
		text = text.Replace("&amp;", "&");
		text = text.Replace("&lt;", "<");
		text = text.Replace("&gt;", ">");

		return text;
	}

	public static string ToTitleCase(this string text)
	{
		if (string.IsNullOrWhiteSpace(text)) return text;
		text = TextInfo.ToTitleCase(text);
		text = ReplaceEnglishParticles(text);
		text = ReplaceJapaneseParticles(text);
		return text;
	}

	private static string ReplaceJapaneseParticles(string text)
	{
		text = text.Replace(" Ha ", " wa ");
		text = text.Replace(" Ga ", " ga ");
		text = text.Replace(" No ", " no ");
		text = text.Replace(" De ", " de ");
		text = text.Replace(" To ", " to ");
		text = text.Replace(" Ni ", " ni ");
		text = text.Replace(" He ", " e ");
		text = text.Replace(" Wo ", " wo ");
		text = text.Replace(" O ", " wo ");
		text = text.Replace(" No ", " no ");
		text = text.Replace(" Ka ", " ka ");
		return text;
	}

	private static string ReplaceEnglishParticles(string text)
	{
		text = text.Replace(" The ", " the ");
		text = text.Replace(" A ", " a ");
		text = text.Replace(" Of ", " of ");
		text = text.Replace(" In ", " in ");
		text = text.Replace(" With ", " with ");
		text = text.Replace(" By ", " by ");
		text = text.Replace(" And ", " and ");
		text = text.Replace(" For ", " for ");
		return text;
	}
}
