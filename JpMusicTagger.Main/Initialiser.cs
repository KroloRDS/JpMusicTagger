using JpMusicTagger.Extensions;
using JpMusicTagger.Google;
using JpMusicTagger.Logging;

namespace JpMusicTagger.Main;

public static class Initialiser
{
	public static async Task<string?> Init()
	{
		var consoleArgs = Environment.GetCommandLineArgs();
		if (CliHelp(consoleArgs)) return null;
		await GoogleApi.Init();

		var entryPath = consoleArgs.GetCliArgValue("-p") ??
			consoleArgs.GetCliArgValue("-path") ??
			Directory.GetCurrentDirectory();

		if (!Directory.Exists(entryPath))
		{
			await Logger.Log($"Directory {entryPath} does not exist");
			return null;
		}

		var logPath = consoleArgs.GetCliArgValue("-l") ??
			consoleArgs.GetCliArgValue("-log") ??
			consoleArgs.GetCliArgValue("-logPath") ?? entryPath;

		if (!Directory.Exists(logPath))
		{
			await Logger.Log($"Directory {logPath} does not exist");
			return null;
		}
		Logger.SetPath(logPath);

		return entryPath;
	}

	private static bool CliHelp(string[] args)
	{
		if (!args.Any(x => x.ToLower() == "-h" || x.ToLower() == "help"))
			return false;

		Console.WriteLine("Parameters:");
		Console.WriteLine("-H / -h / help => Prints this message");
		Console.WriteLine("-P / -p / -path => Path to the directory to be parsed. " +
			"Program expects subdirectories for each artist " +
			"and sub-subdirectories for each of their albums. " +
			"If not present, current path will be used.");
		Console.WriteLine("-L / -l / -log / -logPath => " +
			"Log file save path (folder must exist). " +
			"If not present, current path will be used.");

		return true;
	}
}
