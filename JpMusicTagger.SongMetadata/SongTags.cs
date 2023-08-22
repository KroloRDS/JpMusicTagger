﻿namespace JpMusicTagger.Tags;

public class SongTags
{
	public string Title { get; set; } = string.Empty;
	public string Artist { get; set; } = string.Empty;
	public uint TrackNumber { get; set; }
	public uint DiscNumber { get; set; }
	public string Comment { get; set; } = string.Empty;
	public TimeSpan Length { get; set; } = TimeSpan.Zero;
	public AlbumTags Album { get; set; } = new();
}
