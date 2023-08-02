using JpMusicTagger.CDJapan;
using JpMusicTagger.Logging;
using JpMusicTagger.Main;
using JpMusicTagger.Tags;
using JpMusicTagger.VGMDB;

await Process();

static async Task Process()
{
	var consoleArgs = Environment.GetCommandLineArgs();
	var entryPath = Path.GetDirectoryName(consoleArgs[0]);
	if (consoleArgs.Length > 1)
	{
		entryPath = consoleArgs[1];
		Logger.SetPath(entryPath);
	}

	if (!Directory.Exists(entryPath))
	{
		await Logger.Log($"Directory {entryPath} does not exist");
		return;
	}

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
	var songs = await GetTags(album);
	
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
		song.Tags.Title = await TitleFormatter.Format(song.Tags.Title);
		TagManager.Write(song.Path, song.Tags);
		await FileManager.RenameFile(song.Path, song.Tags);
	}

	await FileManager.RenameFolder(path, songs.First().Album);
}

static async Task<IEnumerable<SongTags>> GetTags(string album)
{
	var songs = await VgmdbApi.GetTags(album);
	if (songs is null || !songs.Any())
		songs = await CdJapanScrapper.GetTags(album);

	return songs;
}
