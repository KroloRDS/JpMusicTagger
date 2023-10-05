using JpMusicTagger.Config;
using JpMusicTagger.Extensions;
using JpMusicTagger.Google;
using JpMusicTagger.Kawazu;
using JpMusicTagger.Logging;

namespace JpMusicTagger.Main;

public static class Initialiser
{
	private class Parameter
	{
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
	}

	private static readonly Parameter EntryPathParam = new()
	{
		Name = "-p",
		Description = "Path to the directory to be parsed. " +
				"Program expects subdirectories for each artist " +
				"and sub-subdirectories for each of their albums. " +
				"Without this parameter current path will be used."
	};
	private static readonly Parameter LogPathParam = new()
	{
		Name = "-l",
		Description = "Path where log file will be saved. " +
				"Without this parameter current path will be used."
	};
	private static readonly Parameter FormatParam = new()
	{
		Name = "-f",
		Description = "Only format existing titles " +
				"instead of downloading them from the internet."
	};
	private static readonly Parameter KawazuDictionaryParam = new()
	{
		Name = "-k",
		Description = "Path containing Kawazu dictionary files. " +
				"Without this parameter current path will be used."
	};
	private static readonly Parameter GoogleApiCredentialsParam = new()
	{
		Name = "-g",
		Description = "Path to json containing Google API refresh token."
	};

	private static readonly IEnumerable<Parameter> Parameters = new[]
	{
		EntryPathParam,
		LogPathParam,
		FormatParam,
		KawazuDictionaryParam,
		GoogleApiCredentialsParam
	};

	public static async Task<string?> Init()
	{
		var consoleArgs = Environment.GetCommandLineArgs();
		if (CliHelp(consoleArgs)) return null;

		GlobalConfig.FormatMode = consoleArgs.Any(
			x => x.ToLower() == FormatParam.Name);

		if (!await InitRomanisationApis(consoleArgs))
		{
			await Logger.Log("No valid romanisation API");
			return null;
		}

		return await InitPaths(consoleArgs);
	}

	private static bool CliHelp(string[] args)
	{
		if (!args.Any(x => x.ToLower() == "-h" || x.ToLower() == "help"))
			return false;

		Console.WriteLine("Parameters:");
		Console.WriteLine("-H / -h / help => Prints this message");
		foreach (var parameter in Parameters)
		{
			Console.WriteLine($"{parameter.Name.ToUpper()} / " +
				$"{parameter.Name.ToLower()} => " +
				$"{parameter.Description}");
		}

		return true;
	}

	private static async Task<bool> InitRomanisationApis(
		string[] consoleArgs)
	{
		if (consoleArgs.Any(x => x.ToLower() == GoogleApiCredentialsParam.Name))
		{
			var googlePath = consoleArgs.GetCliArgValue(GoogleApiCredentialsParam.Name);
			try
			{
				await GoogleApi.Init(googlePath);
			}
			catch (Exception ex)
			{
				await Logger.Log(ex.ToString());
			}
		}

		var kawazuPath = consoleArgs.GetCliArgValue(KawazuDictionaryParam.Name) ??
			Directory.GetCurrentDirectory();

        if (File.Exists(Path.Combine(kawazuPath, "char.bin")))
			KawazuApi.Init(kawazuPath);

		return GlobalConfig.UseGoogleRomanisation ||
			GlobalConfig.UseKawazuRomanisation;
	}

	private static async Task<string?> InitPaths(string[] consoleArgs)
	{
		var entryPath = consoleArgs.GetCliArgValue(EntryPathParam.Name) ??
			Directory.GetCurrentDirectory();

		if (!Directory.Exists(entryPath))
		{
			await Logger.Log($"Directory {entryPath} does not exist");
			return null;
		}

		var logPath = consoleArgs.GetCliArgValue(LogPathParam.Name) ?? entryPath;

		if (!Directory.Exists(logPath))
		{
			await Logger.Log($"Directory {logPath} does not exist");
			return null;
		}
		Logger.SetPath(logPath);

		return entryPath;
	}
}
