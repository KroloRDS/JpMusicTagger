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
		text = text.Replace("&#39;", "'");

		return text;
	}

	public static string ToTitleCase(this string text)
	{
		if (string.IsNullOrWhiteSpace(text)) return text;
		text = text.ToLower();
		text = TextInfo.ToTitleCase(text);
		text = text.Trim();
		text = ReplaceEnglishParticles(text);
		text = ReplaceJapaneseParticles(text);
		return text;
	}

	private static string ReplaceJapaneseParticles(string text)
	{
		text = text.ReplaceParticle("De");
		text = text.ReplaceParticle("Ga");
		text = text.ReplaceParticle("Ha", "wa");
		text = text.ReplaceParticle("He");
		text = text.ReplaceParticle("Ka");
		text = text.ReplaceParticle("Na");
		text = text.ReplaceParticle("Ne");
		text = text.ReplaceParticle("Ni");
		text = text.ReplaceParticle("No");
		text = text.ReplaceParticle("Mo");
		text = text.ReplaceParticle("O");
		text = text.ReplaceParticle("To");
		text = text.ReplaceParticle("Wa");
		text = text.ReplaceParticle("Wo");

		return text;
	}

	private static string ReplaceEnglishParticles(string text)
	{
		text = text.ReplaceParticle("A");
		text = text.ReplaceParticle("And");
		text = text.ReplaceParticle("By");
		text = text.ReplaceParticle("For");
		text = text.ReplaceParticle("In");
		text = text.ReplaceParticle("Of");
		text = text.ReplaceParticle("The");
		text = text.ReplaceParticle("With");

		text = text.Replace("'S", "'s");
		text = text.Replace("'Ve", "'ve");
		text = text.Replace("'Nt", "'nt");

		return text;
	}

	private static string ReplaceParticle(this string text,
		string particle, string? replacement = null)
	{
		var particleUpper = char.ToUpper(particle[0]) + particle[1..].ToLower();
		var particleLower = string.IsNullOrWhiteSpace(replacement) ?
			 particle.ToLower() : replacement.ToLower();

		text = text.Replace($" {particleUpper} ", $" {particleLower} ");
		if (text.EndsWith($" {particleUpper}"))
		{
			var index = text.LastIndexOf(particleUpper);
			text = text[..index] + particleLower;
		}

		return text;
	}
}
