﻿using JpMusicTagger.CDJapan;
using JpMusicTagger.Logging;
using JpMusicTagger.Main;
using JpMusicTagger.Tags;
using JpMusicTagger.VGMDB;

await Process();

static async Task Process()
{
	var consoleArgs = new[] { "D:\\Temp\\test" };//Environment.GetCommandLineArgs();

	var entryPath = consoleArgs[0];
	if (!Directory.Exists(entryPath))
	{
		await Logger.Log($"Directory {entryPath} does not exist");
		return;
	}

	var logPath = consoleArgs.Length > 1 ? consoleArgs[1] : entryPath;
	if (!Directory.Exists(logPath))
	{
		await Logger.Log($"Directory {logPath} does not exist");
		return;
	}
	Logger.SetPath(entryPath);

	foreach (var artistDir in Directory.GetDirectories(entryPath))
	{
		var artist = Path.GetFileNameWithoutExtension(artistDir);
		foreach (var albumDir in Directory.GetDirectories(artistDir))
		{
			await ProcessAlbum(artist, albumDir);
		}
	}
}

static async Task ProcessAlbum(string artist, string path)
{
	var album = Path.GetFileNameWithoutExtension(path);
	var songs = await GetTags(album, artist);
	
	if (songs is null || !songs.Any())
	{
		await Logger.Log("Failed to get album data", album, artist);
		return;
	}

	var songFiles = DirectoryMatcher.Match(songs, path);
	if (songFiles is null || !songFiles.Any())
	{
		await Logger.Log("Album data mismatch", album, artist);
		return;
	}

	foreach (var song in songFiles)
	{
		var originalFileName = Path.GetFileNameWithoutExtension(song.Path);
		song.Tags.Comment = song.Tags.Title;
		song.Tags.Title = await TitleFormatter.Format(song.Tags.Title);
		TagManager.Write(song.Path, song.Tags);
		await FileManager.RenameFile(song.Path, song.Tags);
	}

	//await FileManager.RenameFolder(path, songs.First().Album);
}

static async Task<IEnumerable<SongTags>> GetTags(string album, string artist)
{
	var songs = await VgmdbScrapper.GetTags(album);
	if (songs is null || !songs.Any())
		songs = await CdJapanScrapper.GetTags(album, artist);

	return songs;
}
