using System.Text;

namespace JpMusicTagger.Logging;

public static class Logger
{
	private static string LogPath = AppDomain.CurrentDomain.BaseDirectory;
	private static readonly string LogFileName = SetLogFileName();

	private static string SetLogFileName()
	{
		var now = DateTime.Now.ToString("yy.MM.dd HH-mm-ss");
		return now + " JpMusicTagger.log";
	}

	public static void SetPath(string path) => LogPath = path;

	public static async Task Log(string message,
		string artist = "", string album = "")
	{
		var log = BuildLogText(message, artist, album);
		try
		{
			var path = Path.Combine(LogPath, LogFileName);
			await File.AppendAllTextAsync(path, log);
		}
		finally
		{
			Console.Write(log);
		}
	}
	private static string BuildLogText(string message,
		string artist = "", string album = "")
	{
		var sb = new StringBuilder();
		if (!string.IsNullOrEmpty(artist)) sb.AppendLine("Artist: " + artist);
		if (!string.IsNullOrEmpty(album)) sb.AppendLine("Album: " + album);
		sb.AppendLine(message + Environment.NewLine);
		var log = sb.ToString();
		return log;
	}
}
