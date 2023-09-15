using JpMusicTagger.CDJapan;
using JpMusicTagger.Logging;
using JpMusicTagger.Main;
using JpMusicTagger.Tags;
using JpMusicTagger.Extensions;
using JpMusicTagger.VGMDB;

var path = await Initialiser.Init();
if (path is not null) await Process(path);

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
		song.Tags.Comment = song.Tags.Comment.HasJapaneseChars() ?
			song.Tags.Title : string.Empty;
		song.Tags.Title = await TitleFormatter.Format(song.Tags.Title);
		TagManager.Write(song.Path, song.Tags);
		await FileManager.RenameFile(song.Path, song.Tags);
	}

	await FileManager.RenameFolder(path, songs.First().Album);
	Console.WriteLine($"Done processing {artist}/{album}");
}

static async Task<IEnumerable<SongTags>> GetTags(string album, string artist)
{
	(var catalogNumber, album) = GetCatalogNumberAndName(album);
	var songs = await GetTagsFromVgmdb(album, catalogNumber);

	if (songs is null || !songs.Any())
		songs = await GetTagsFromCdJapan(album, artist, catalogNumber);

	return songs;
}

static async Task<IEnumerable<SongTags>> GetTagsFromVgmdb(
	string album, string? catalogNumber = null)
{
	var songs = Enumerable.Empty<SongTags>();
	if (!string.IsNullOrWhiteSpace(catalogNumber))
		songs = await VgmdbScrapper.GetTags(catalogNumber);

	if (songs is null || !songs.Any())
		songs = await VgmdbScrapper.GetTags(album);

	return songs;
}

static async Task<IEnumerable<SongTags>> GetTagsFromCdJapan(
	string album, string? artist = null, string? catalogNumber = null)
{
	var songs = Enumerable.Empty<SongTags>();
	if (!string.IsNullOrWhiteSpace(catalogNumber))
		songs = await CdJapanScrapper.GetTags(catalogNumber, artist);

	if (songs is null || !songs.Any())
		songs = await CdJapanScrapper.GetTags(album, artist);

	return songs;
}

static (string? CatalogNumber, string Name) GetCatalogNumberAndName(string album)
{
	var index = album.IndexOf(']');
	if (index == -1 || index == album.Length - 1) return (null, album.Trim());

	var name = album[(index + 1)..];
	var catalogNumer = album[1..index];
	index = catalogNumer.IndexOf("~");
	if (index != -1) catalogNumer = catalogNumer[..index];
	
	return (catalogNumer.Trim(), name.Trim());
}
