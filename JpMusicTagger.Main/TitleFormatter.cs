using JpMusicTagger.Google;
using JpMusicTagger.Logging;
using JpMusicTagger.Utils;
using System.Text;
using System.Text.RegularExpressions;

namespace JpMusicTagger.Main;

public static partial class TitleFormatter
{
	[GeneratedRegex(" +")]
	private static partial Regex MultipleSpaces();

	[GeneratedRegex("!+")]
	private static partial Regex MultipleBangs();

	public static async Task<string> Format(string text)
	{
		text = ReplaceSymbols(text);
		text = text.Trim();
		text = await Romanise(text);
		text = text.ToTitleCase();
		text = ReplaceVowels(text);
		text = FixBraceSpaces(text);
		text = text.Trim();
		return text;
	}

	private static async Task<string> Romanise(string input)
	{
		if (string.IsNullOrWhiteSpace(input)) return input;
		var output = new StringBuilder();
		var buffer = new StringBuilder();
		var currentType = input[0].Type();
		var length = input.Length;

		for (var i = 0; i < length; i++)
		{
			buffer.Append(input[i]);
			TextType? nextType = i + 1 < length ? input[i + 1].Type() : null;
			if (nextType != currentType)
			{
				var bufferStr = buffer.ToString();
				var converted = await ConvertBuffer(bufferStr, currentType);
				output.Append(' ');
				output.Append(converted);
				currentType = nextType ?? currentType;
				buffer.Clear();
			}
		}
		return output.ToString();
	}

	private static async Task<string> ConvertBuffer(string text, TextType type)
	{
		return type switch
		{
			TextType.Katakana => await GoogleApi.Translate(text),
			TextType.Kanji => await GoogleApiRomanise(text),
			_ => text,
		};
	}

	private static async Task<string> GoogleApiRomanise(string text)
	{
		var romanised = await GoogleApi.Romanise(text);
		if (romanised.ToCharArray().Any(x => x.Type() == TextType.Kanji))
		{
			await Logger.Log($"Text {text} was not romanised");
		}
		return romanised;
	}

	private static string ReplaceSymbols(string text)
	{
		text = text.Replace('「', '[');
		text = text.Replace('」', ']');

		text = text.Replace('『', '[');
		text = text.Replace('』', ']');

		text = text.Replace('【', '[');
		text = text.Replace('】', ']');

		text = text.Replace('［', '[');
		text = text.Replace('］', ']');

		text = text.Replace('（', '(');
		text = text.Replace('）', ')');

		text = text.Replace("＜", "(");
		text = text.Replace("＞", ")");

		text = text.Replace("<", "(");
		text = text.Replace(">", ")");

		text = text.Replace('〜', '~');

		text = text.Replace('\t', ' ');
		text = text.Replace('\r', ' ');
		
		text = MultipleSpaces().Replace(text, " ");
		text = MultipleBangs().Replace(text, "!");

		return text;
	}

	private static string ReplaceVowels(string text)
	{
		//https://www.youtube.com/watch?v=Hv6RbEOlqRo
		text = text.Replace("ā", "aa");
		text = text.Replace("ē", "ei");
		text = text.Replace("ī", "ii");

		text = text.Replace("ō", "ou");
		text = text.Replace('ô', 'o');

		text = text.Replace("ū", "uu");
		text = text.Replace('û', 'u');

		return text;
	}

	private static string FixBraceSpaces(string text)
	{
		text = text.Replace(" (", "(");
		text = text.Replace("( ", "(");
		text = text.Replace("(", " (");

		text = text.Replace(" )", ")");
		text = text.Replace("( ", ")");
		text = text.Replace(")", ") ");

		text = text.Replace(" [", "[");
		text = text.Replace("[ ", "[");
		text = text.Replace("[", " [");

		text = text.Replace(" ]", "]");
		text = text.Replace("] ", "]");
		text = text.Replace("]", "] ");

		return text;
	}
}