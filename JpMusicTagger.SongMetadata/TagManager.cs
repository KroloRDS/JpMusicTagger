namespace JpMusicTagger.Tags;

public static class TagManager
{
	public static SongTags Get(string path)
	{
		throw new NotImplementedException();
	}

	public static void Write(string path, SongTags songMetadata)
	{
		throw new NotImplementedException();
	}

	public static void Clear(string path)
	{
		Write(path, new());
	}
}
