namespace JpMusicTagger.Utils;

public static class CliHelper
{
	public static bool Help(string[] args)
	{
		if (!args.Any(x => x == "-h" || x == "-H" || x.ToLower() == "help"))
			return false;

		Console.WriteLine("Parameters:");
		Console.WriteLine("-H / -h / help => Prints this message");
		Console.WriteLine("-P / -p / -path => Path to the directory to be parsed. " +
			"Program expects subdirectories for each artist " +
			"and sub-subdirectories for each of their albums");
		Console.WriteLine("-L / -l / -log / -logPath => Log file save path (folder must exist)");
		Console.WriteLine("-T / -t / -token => REQUIRED: Google Cloud token");
		Console.WriteLine("-I / -i / -id => REQUIRED: Google Cloud Project ID");

		return true;
	}

	public static string? GetParameter(string name, string[] args)
	{
		var index = args.ToList().IndexOf(name.ToLower());
		return index != -1 && args.Length > index + 1 ? args[index + 1] : null;
	}
}
