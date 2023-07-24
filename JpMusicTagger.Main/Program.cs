using JpMusicTagger.CDJapan;
using JpMusicTagger.Main;
using JpMusicTagger.Tags;
using JpMusicTagger.VGMDB;

await Process();

static async Task Process()
{
	var consoleArgs = Environment.GetCommandLineArgs();
	var entryPath = consoleArgs.Length > 1 ? consoleArgs[1] :
		Path.GetDirectoryName(consoleArgs[0]);

	if (!Directory.Exists(entryPath))
	{
		Logger.Log($"Directory {entryPath} does not exist");
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
		Logger.Log("Failed to get album data", album, artist);
		return;
	}

	var songFiles = DirectoryMatcher.Match(songs, path);
	if (songFiles is null || !songFiles.Any())
	{
		Logger.Log("Album data mismatch", album, artist);
		return;
	}

	foreach (var song in songFiles)
	{
		song.Tags.Title = await TitleFormatter.Format(song.Tags.Title);
		TagManager.Write(song.Path, song.Tags);
		FileManager.RenameFile(song.Path, song.Tags);
	}

	FileManager.RenameFolder(path, songs.First().Album.CatalogNumber);
}

static async Task<IEnumerable<SongTags>> GetTags(string album)
{
	var songs = await VgmdbApi.GetTags(album);
	if (songs is null || !songs.Any())
		songs = await CdJapanScrapper.GetTags(album);

	return songs;
}
