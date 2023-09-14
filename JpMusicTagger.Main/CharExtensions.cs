namespace JpMusicTagger.Main;

public enum TextType
{
	Katakana,
	Kanji,
	Other
}

public static class CharExtensions
{
	//https://www.ling.upenn.edu/courses/Spring_2003/ling538/UnicodeRanges.html

	//http://www.unicode.org/charts/PDF/U3040.pdf
	private static readonly CharRange Hiragana = new('\u3040', '\u309F');

	//http://www.unicode.org/charts/PDF/U30A0.pdf
	private static readonly CharRange Katakana = new('\u30A0', '\u30FF');

	//http://www.unicode.org/charts/PDF/U31F0.pdf
	private static readonly CharRange KatakanaExt = new('\u31F0', '\u31FF');

	//http://www.unicode.org/charts/PDF/U4E00.pdf
	private static readonly CharRange Kanji = new('\u4E00', '\u9FFF');

	public static TextType Type(this char c)
	{
		if (Hiragana.Check(c) || Kanji.Check(c))
			return TextType.Kanji;
		if (Katakana.Check(c) || KatakanaExt.Check(c))
			return TextType.Katakana;
		return TextType.Other;
	}

	private class CharRange
	{
		public char Start;
		public char End;

		public CharRange(char start, char end)
		{
			if (start < 0)
				throw new ArgumentOutOfRangeException(nameof(start));
			if (end <= start)
				throw new ArgumentOutOfRangeException(nameof(end));

			Start = start;
			End = end;
		}

		public bool Check(char c) => c >= Start && c <= End;
	}
}
