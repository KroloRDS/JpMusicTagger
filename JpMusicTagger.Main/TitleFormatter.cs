using System.Globalization;

namespace JpMusicTagger.Main;

public static class TitleFormatter
{
	private static readonly TextInfo TextInfo =
		new CultureInfo("en-US", false).TextInfo;

	public static async Task<string> Format(string input)
	{
		throw new NotImplementedException();
	}

	private static async Task<string> Romanise(string input)
	{
		var output = "";
		var japBuffer = "";

		foreach (var c in input)
		{
			//https://www.ling.upenn.edu/courses/Spring_2003/ling538/UnicodeRanges.html
			if (c >= 0x2E80 && c <= 0x9FAF)
			{
				japBuffer += c;
				continue;
			}

			if (!string.IsNullOrEmpty(japBuffer))
			{
				output += await RomanisePhrase(japBuffer);
				japBuffer = "";
			}
			output += c;
		}
		return output;
	}

	private static async Task<string> RomanisePhrase(string input)
	{
		try
		{
			var output = await Romaniser.Romaniser.Convert(input);
			output = output.Replace('\"', '\'').Trim();
			return TextInfo.ToTitleCase(output);
		}
		catch
		{
			return input;
		}
	}
}
