using ATL;

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
		var track = new Track(path);
		var tags = new SongTags
		{
			Artist = track.Artist,
			Comment = track.Comment,
			DiscNumber = track.DiscNumber,
			Length = TimeSpan.FromSeconds(track.Duration),
			Title = track.Title,
			TrackNumber = track.TrackNumber,
			Album = new AlbumTags
			{
				Artist = track.AlbumArtist,
				Composer = track.Composer,
				Name = track.Album,
				Year = track.Year
			}
		};
		return tags;
	}

	public static void Write(string path, SongTags songMetadata)
	{
		var track = new Track(path);

		track.Artist = songMetadata.Artist;
		track.AlbumArtist = songMetadata.Album.Artist;
		track.Composer = songMetadata.Album.Composer;
		
		track.Album = songMetadata.Album.Name;
		track.Title = songMetadata.Title;
		if (HasPositiveValue(songMetadata.Album.Year))
			track.Year = songMetadata.Album.Year;

		track.Comment = songMetadata.Comment;
		if (HasPositiveValue(songMetadata.TrackNumber))
			track.TrackNumber = songMetadata.TrackNumber;
		if (HasPositiveValue(songMetadata.DiscNumber))
			track.DiscNumber = songMetadata.DiscNumber;

		track.Save();
	}

	private static bool HasPositiveValue(int? number) =>
		number.HasValue && number.Value > 0;
}
