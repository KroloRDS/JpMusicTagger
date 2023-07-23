namespace JpMusicTagger.Tags;

public class SongTags
{
	public string Title { get; set; } = string.Empty;
	public string TrackNumber { get; set; } = string.Empty;
	public string DiscNumber { get; set; } = string.Empty;
	public string Comment { get; set; } = string.Empty;
	public TimeSpan Length { get; set; } = TimeSpan.Zero;
	public AlbumTags Album { get; set; } = new();
}
