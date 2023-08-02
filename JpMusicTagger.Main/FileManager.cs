using JpMusicTagger.Logging;
using JpMusicTagger.Tags;

namespace JpMusicTagger.Main;

public static class FileManager
{
	public static async Task RenameFolder(string path, AlbumTags album)
	{
		if (string.IsNullOrEmpty(album.CatalogNumber)) return;

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

	public static async Task RenameFile(string path, SongTags tags)
	{
		var newName = GetNewFileName(tags);
		try
		{
			var info = new FileInfo(path);
			info.MoveTo(info.Directory!.FullName + "\\" + newName);
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
		var newName = tags.TrackNumber + '.' + tags.Title;
		if (!string.IsNullOrEmpty(tags.DiscNumber))
			newName = tags.DiscNumber + '.' + newName;
		return SanitizeText(newName);
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
