﻿using System.Text;

namespace JpMusicTagger.Main;

public static class Logger
{
	private static string LogPath = AppDomain.CurrentDomain.BaseDirectory;
	private static readonly string LogFileName = 
		typeof(Program).Assembly.GetName().Name + ".log";

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
		if (!string.IsNullOrEmpty(album)) sb.AppendLine("Artist: " + album);
		sb.AppendLine(message + Environment.NewLine);
		var log = sb.ToString();
		return log;
	}
}
