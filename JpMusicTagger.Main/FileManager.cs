using JpMusicTagger.Tags;

namespace JpMusicTagger.Main;

public static class FileManager
{
	public static async Task RenameFolder(string path, string catalogNumber)
	{
		if (string.IsNullOrEmpty(catalogNumber)) return;

		var info = new DirectoryInfo(path);
		var prefix = $"[{catalogNumber}] ";
		if (info.Name.StartsWith(prefix)) return;

		try
		{
			info.MoveTo(info.Parent!.FullName + "\\" + prefix + info.Name);
		}
		catch
		{
			var msg = $"Failed to rename folder: {path} with prefix {prefix}";
			var artist = info?.Parent?.Name ?? string.Empty;
			var album = info?.Name ?? string.Empty;
			await Logger.Log(msg, artist, album);
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
		return newName;
	}
}
