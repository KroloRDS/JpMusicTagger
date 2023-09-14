using JpMusicTagger.Logging;
using JpMusicTagger.Tags;

namespace JpMusicTagger.Main;

public static class FileManager
{
	public static async Task RenameFolder(string path, AlbumTags album)
	{
		if (!IsCatalogNumber(album.CatalogNumber)) return;

		var info = new DirectoryInfo(path);
		var prefix = $"[{album.CatalogNumber}] ";
		if (info.Name.StartsWith(prefix)) return;

		var albumName = SanitizeText(album.Name);
		try
		{
			info.MoveTo(info.Parent!.FullName + "\\" + prefix + albumName);
		}
		catch
		{
			var msg = $"Failed to rename folder: {path} with prefix {prefix}";
			var artist = info?.Parent?.Name ?? string.Empty;
			var dir = info?.Name ?? string.Empty;
			await Logger.Log(msg, artist, dir);
		}
	}

	private static bool IsCatalogNumber(string? text)
	{
		if (text is null) return false;
		if (text.Length < 6 || text.Length > 11) return false;
		for (var i = 0; i < 4; i++)
		{
			if (!char.IsAsciiLetterUpper(text[i])) return false;
		}
		if (text[4] != '-') return false;
		for (var i = 6; i < text.Length - 2; i++)
		{
			if (!char.IsDigit(text[i])) return false;
		}
		if (text[^2] != '~' && !char.IsDigit(text[^2])) return false;
		return char.IsDigit(text[^1]);
	}

	public static async Task RenameFile(string path, SongTags tags)
	{
		var newName = GetNewFileName(tags);
		var ext = Path.GetExtension(path).ToLower();
		try
		{
			var info = new FileInfo(path);
			info.MoveTo(info.Directory!.FullName + "\\" + newName + ext);
		}
		catch
		{
			var artist = Directory.GetParent(path)?.Parent?.Name ?? string.Empty;
			var msg = $"Failed to rename: {path} to {newName}";
			await Logger.Log(msg, artist, tags.Album.Name);
		}
	}

	private static string GetNewFileName(SongTags tags)
	{
		var trackNumber = GetNumberPrefix(tags.TrackNumber);
		var discNumber = GetNumberPrefix(tags.DiscNumber, false);
		var newName = discNumber + trackNumber + tags.Title;
		return SanitizeText(newName);
	}

	private static string GetNumberPrefix(int? number, bool leadingZero = true)
	{
		if (!number.HasValue || number == 0) return string.Empty;
		var text = number.ToString();
		if (leadingZero && number < 10) text = '0' + text;
		return text + '.';
	}

	private static string SanitizeText(string text)
	{
		text = text.Replace('/', '⧸');
		text = text.Replace('\\', '⧹');
		text = text.Replace(":", "꞉");
		text = text.Replace("*", "⁎");
		text = text.Replace("?", "？");
		text = text.Replace("\"", "\'\'");
		text = text.Replace("<", "(");
		text = text.Replace(">", ")");
		text = text.Replace("|", "⏐");
		return text;
	}
}
