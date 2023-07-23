using JpMusicTagger.CDJapan;
using JpMusicTagger.Main;
using JpMusicTagger.Tags;
using JpMusicTagger.VGMDB;

await Process();

async Task Process()
{
	var consoleArgs = Environment.GetCommandLineArgs();
	var entryPath = consoleArgs.Length > 1 ? consoleArgs[1] :
		Path.GetDirectoryName(consoleArgs[0]);

	if (!Directory.Exists(entryPath))
	{
		Console.WriteLine($"ERROR: Directory {entryPath} does not exist.");
		return;
	}

	foreach (var dir in Directory.GetDirectories(entryPath))
	{
		await ProcessArtist(dir);
	}
}

async Task ProcessArtist(string path)
{
	var artist = Path.GetFileNameWithoutExtension(path);
	foreach (var dir in Directory.GetDirectories(path))
	{
		await ProcessAlbum(artist, dir);
	}
}

async Task ProcessAlbum(string artist, string path)
{
	var album = Path.GetFileNameWithoutExtension(path);
	var songs = await GetTags(album);
	
	if (songs is null || !songs.Any())
	{
		Logger.Log(album, artist, "Failed to get album data");
		return;
	}

	var songFiles = DirectoryMatcher.Match(songs, path);
	if (songFiles is null || !songFiles.Any())
	{
		Logger.Log(album, artist, "Album data mismatch");
		return;
	}

	foreach (var song in songFiles)
	{
		song.Tags.Title = await TitleFormatter.Format(song.Tags.Title);
		TagManager.Write(song.Path, song.Tags);
		RenameFile();
	}
	RenameFolder();
}

static async Task<IEnumerable<SongTags>> GetTags(string album)
{
	var songs = await VgmdbApi.GetTags(album);
	if (songs is null || !songs.Any())
		songs = await CdJapanScrapper.GetTags(album);

	return songs;
}

void RenameFile(string file)
{
	var fileInfo = new FileInfo(file);
	try
	{
		fileInfo.MoveTo(fileInfo.Directory!.FullName + "\\" + romanised);
	}
	catch
	{
		log += "ERROR when renaming to: " + romanised;
	}
	log += file + " => " + romanised + "\n";
}
