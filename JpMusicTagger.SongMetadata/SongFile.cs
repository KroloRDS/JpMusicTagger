namespace JpMusicTagger.Tags;

public class SongFile
{
	public string Path { get; set; } = string.Empty;
	public SongTags Tags { get; set; } = new();
}
