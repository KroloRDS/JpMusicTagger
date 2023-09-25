using JpMusicTagger.Extensions;
using JpMusicTagger.Logging;
using JpMusicTagger.Tags;

namespace JpMusicTagger.Main;

public static class FileManager
{
	private static readonly HashSet<string> AudioExtensions = new()
	{
		".mp3", ".ogg", ".flac", ".wav", ".m4a"
	};

	public static bool IsAudioFile(string path)
	{
		var extension = Path.GetExtension(path).ToLower();
		return AudioExtensions.Contains(extension);
	}

	public static string GetTitleWithoutTrackNumber(string path)
	{
		var name = Path.GetFileNameWithoutExtension(path);
		if (name.Length <= 3) return name;

		var startsWithTrackNumber = char.IsAsciiDigit(name[0]) &&
			char.IsAsciiDigit(name[1]) && name[2] == '.';

		return startsWithTrackNumber ?
			name[3..].Trim() : name.Trim();
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
		return newName.ReplaceFilenameInvalidChars();
	}

	private static string GetNumberPrefix(int? number, bool leadingZero = true)
	{
		if (!number.HasValue || number == 0) return string.Empty;
		var text = number.ToString();
		if (leadingZero && number < 10) text = '0' + text;
		return text + '.';
	}
}
