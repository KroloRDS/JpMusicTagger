﻿using JpMusicTagger.CDJapan;
using JpMusicTagger.Logging;
using JpMusicTagger.Main;
using JpMusicTagger.Tags;
using JpMusicTagger.Utils;
using JpMusicTagger.VGMDB;

var path = await Init();
if (path is not null) await Process(path);

static async Task<string?> Init()
{
	var consoleArgs = Environment.GetCommandLineArgs();
	if (CliHelper.Help(consoleArgs)) return null;

	var entryPath = CliHelper.GetParameter("-p", consoleArgs) ??
		CliHelper.GetParameter("-path", consoleArgs) ??
		Directory.GetCurrentDirectory();

	if (!Directory.Exists(entryPath))
	{
		await Logger.Log($"Directory {entryPath} does not exist");
		return null;
	}

	var logPath = CliHelper.GetParameter("-l", consoleArgs) ??
		CliHelper.GetParameter("-log", consoleArgs) ??
		CliHelper.GetParameter("-logPath", consoleArgs) ?? entryPath;

	if (!Directory.Exists(logPath))
	{
		await Logger.Log($"Directory {logPath} does not exist");
		return null;
	}
	Logger.SetPath(logPath);

	return entryPath;
}

static async Task Process(string entryPath)
{
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
		await Logger.Log("Failed to get album data", artist, album);
		return;
	}

	var songFiles = DirectoryMatcher.Match(songs, path);
	if (songFiles is null || !songFiles.Any())
	{
		await Logger.Log("Album data mismatch", artist, album);
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

	await FileManager.RenameFolder(path, songs.First().Album);
}

static async Task<IEnumerable<SongTags>> GetTags(string album, string artist)
{
	var songs = await VgmdbScrapper.GetTags(album);
	if (songs is null || !songs.Any())
		songs = await CdJapanScrapper.GetTags(album, artist);

	return songs;
}
