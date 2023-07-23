namespace JpMusicTagger.Tags;

public class AlbumTags
{
	public string CatalogNumber { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string Composer { get; set; } = string.Empty;
	public DateOnly ReleaseDate { get; set; }
}
