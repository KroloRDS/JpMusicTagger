using JpMusicTagger.Romanising;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace JpMusicTagger.Main;

public static partial class TitleFormatter
{
	private static readonly TextInfo TextInfo =
		new CultureInfo("en-US", false).TextInfo;

	[GeneratedRegex(" +")]
	private static partial Regex MultipleSpaces();

	[GeneratedRegex("!+")]
	private static partial Regex MultipleBangs();

	public static async Task<string> Format(string text)
	{
		text = text.Trim();
		text = await Romanise(text);
		text = TextInfo.ToTitleCase(text);
		text = ReplaceSymbols(text);
		text = ReplaceEnglishParticles(text);
		text = ReplaceJapaneseParticles(text);
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
			TextType.Katakana => await GoogleTranslate.Translate(text),
			TextType.Kanji => await Romaniser.Convert(text),
			_ => text,
		};
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
}