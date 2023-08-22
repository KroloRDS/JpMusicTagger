﻿namespace JpMusicTagger.Tags;

public class DirectoryMatcher
{
	private static readonly HashSet<string> AudioExtensions = new()
	{
		".mp3", ".ogg", ".flac", ".wav", ".m4a"
	};

	public static IEnumerable<(string Path, SongTags Tags)>? Match(
		IEnumerable<SongTags> tags, string path)
	{
		var files = GetFiles(path);
		if (tags.Count() != files.Count()) return null;
		var joined = Join(files, tags);

		if (!CheckLengths(joined)) return null;

		var result = new List<(string Path, SongTags Tags)>();
		foreach (var song in joined) result.Add(new (song.Path, song.Tags));
		return result;
	}

	private static IEnumerable<string> GetFiles(string path)
	{
		var files = Directory.GetFiles(path);
		var audioFiles = files.Where(IsAudioFile);
		return audioFiles;
	}

	private static bool IsAudioFile(string path)
	{
		var extension = Path.GetExtension(path).ToLower();
		return AudioExtensions.Contains(extension);
	}

	private static IEnumerable<SongFile> Join(
		IEnumerable<string> paths, IEnumerable<SongTags> tags)
	{
		var tagsArr = tags.ToArray();
		var pathsArr = paths.ToArray();
		var files = new List<SongFile>();
		for (int i = 0; i < tagsArr.Length; i++)
		{
			files.Add(new SongFile
			{
				Path = pathsArr[i],
				Tags = MergeTags(pathsArr[i], tagsArr[i])
			});
		}

		return files;
	}

	private static bool CheckLengths(IEnumerable<SongFile> files)
	{
		var epsilon = TimeSpan.FromSeconds(2);
		foreach (var file in files)
		{
			var lengthFromTags = file.Tags.Length;
			if (lengthFromTags == TimeSpan.Zero) continue;

			var diff = lengthFromTags - TagManager.GetSongLength(file.Path);
			if (diff < TimeSpan.Zero) diff *= -1;
			if (diff > epsilon) return false;
		}

		return true;
	}

	private static SongTags MergeTags(string path, SongTags tags)
	{
		var fileTags = TagManager.Get(path);

		if (tags.TrackNumber == 0) tags.TrackNumber = fileTags.TrackNumber;
		if (tags.DiscNumber == 0) tags.DiscNumber = fileTags.DiscNumber;
		if (tags.Album.Year == 0) tags.Album.Year = fileTags.Album.Year;

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

	private class SongFile
	{
		public string Path { get; set; } = string.Empty;
		public SongTags Tags { get; set; } = new();
	}
}
