namespace JpMusicTagger.Tags;

public static class TagManager
{
	public static TimeSpan GetSongLength(string path)
	{
		var file = TagLib.File.Create(path);
		TimeSpan duration = file.Properties.Duration;
		return duration;
	}

	public static SongTags Get(string path)
	{
		var file = TagLib.File.Create(path);
		var tags = new SongTags
		{
			Artist = file.Tag.JoinedPerformers,
			Comment = file.Tag.Comment,
			DiscNumber = file.Tag.Disc,
			Length = file.Properties.Duration,
			Title = file.Tag.Title,
			TrackNumber = file.Tag.Track,
			Album = new AlbumTags
			{
				Artist = file.Tag.JoinedAlbumArtists,
				Composer = file.Tag.JoinedComposers,
				Name = file.Tag.Album,
				Year = file.Tag.Year
			}
		};
		return tags;
	}

	public static void Write(string path, SongTags songMetadata)
	{
		var file = TagLib.File.Create(path);

		file.Tag.Performers = songMetadata.Artist.Split(", ");
		file.Tag.AlbumArtists = songMetadata.Album.Artist.Split(", ");
		file.Tag.Composers = songMetadata.Album.Composer.Split(", ");
		
		file.Tag.Album = songMetadata.Album.Name;
		if (songMetadata.Album.Year > 0)
			file.Tag.Year = songMetadata.Album.Year;

		file.Tag.Comment = songMetadata.Comment;
		if (songMetadata.TrackNumber > 0)
			file.Tag.Track = songMetadata.TrackNumber;
		if (songMetadata.DiscNumber > 0)
			file.Tag.Disc = songMetadata.DiscNumber;

		file.Save();
	}

	public static void Clear(string path)
	{
		Write(path, new());
	}
}
