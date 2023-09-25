using JpMusicTagger.Extensions;
using JpMusicTagger.Logging;
using JpMusicTagger.Main;

namespace JpMusicTagger.Tags;

public class DirectoryManager
{
	public static IEnumerable<string> GetAudioFiles(string path)
	{
		var files = Directory.GetFiles(path);
		var audioFiles = files.Where(FileManager.IsAudioFile);
		return audioFiles;
	}

	public static async Task RenameFolder(string path, AlbumTags album)
	{
		if (!IsCatalogNumber(album.CatalogNumber)) return;

		var info = new DirectoryInfo(path);
		var prefix = $"[{album.CatalogNumber}] ";
		if (info.Name.StartsWith(prefix)) return;

		var albumName = album.Name.ReplaceFilenameInvalidChars();
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

	public static IEnumerable<SongFile>? JoinTagsAndPaths(
		IEnumerable<SongTags> tags, string dirPath)
	{
		var filePaths = GetAudioFiles(dirPath);
		if (tags.Count() != filePaths.Count()) return null;

		var files = new List<SongFile>();
		var i = 0;
		foreach (var song in tags)
		{
			var filePath = filePaths.ElementAt(i);
			if (!CheckLength(song, filePath)) return null;
			files.Add(new SongFile
			{
				Path = filePath,
				Tags = MergeTags(filePath, song)
			});
			i++;
		}

		return files;
	}

	private static bool CheckLength(SongTags tags, string path)
	{
		if (tags.Length == TimeSpan.Zero) return true;

		var epsilon = TimeSpan.FromSeconds(4);
		var diff = tags.Length - TagManager.GetSongLength(path);
		return diff > epsilon || diff < -epsilon;
	}

	private static SongTags MergeTags(string path, SongTags tags)
	{
		var fileTags = TagManager.Get(path);

		if (IsNullOrZero(tags.TrackNumber)) tags.TrackNumber = fileTags.TrackNumber;
		if (IsNullOrZero(tags.DiscNumber)) tags.DiscNumber = fileTags.DiscNumber;
		if (IsNullOrZero(tags.Album.Year)) tags.Album.Year = fileTags.Album.Year;

		if (string.IsNullOrWhiteSpace(tags.Artist))
			tags.Artist = fileTags.Artist;
		if (string.IsNullOrWhiteSpace(tags.Album.Name))
			tags.Album.Name = fileTags.Album.Name;
		if (string.IsNullOrWhiteSpace(tags.Album.Artist))
			tags.Album.Artist = fileTags.Album.Artist;
		if (string.IsNullOrWhiteSpace(tags.Album.Composer))
			tags.Album.Composer = fileTags.Album.Composer;

		return tags;
	}

	private static bool IsNullOrZero(int? number) => !number.HasValue || number == 0;
}
