using Kawazu;
using System.Globalization;

var audioExtensions = new HashSet<string>{ ".mp3", ".ogg", ".flac", ".wav", ".m4a" };
var textInfo = new CultureInfo("en-US", false).TextInfo;
var converter = new KawazuConverter();
var log = "";
var entryPath = "";
var logPath = "";

if (!Setup()) return;
RenameInFolder(entryPath).Wait();
SaveLog();

bool Setup()
{
	Console.WriteLine("Please input entry path:");
	entryPath = Console.ReadLine();
	if (!Directory.Exists(entryPath))
	{
		Console.WriteLine("ERROR: Folder does not exist.");
		return false;
	}

	Console.WriteLine("Please input log path:");
	logPath = Console.ReadLine();
	return true;
}

Task RenameInFolder(string path)
{
	foreach (var file in Directory.GetFiles(path))
	{
		RenameFile(file);
	}

	var tasks = Directory.GetDirectories(path)
		.Select(dir => RenameInFolder(dir));
	return Task.WhenAll(tasks);
}

void RenameFile(string file)
{
	var ext = Path.GetExtension(file).ToLower();
	if (!audioExtensions.Contains(ext)) return;

	var name = Path.GetFileName(file);
	var romanised = Romanise(name);
	if (romanised == name) return;

	var fileInfo = new FileInfo(file);
	try
	{
		fileInfo.MoveTo(fileInfo.Directory!.FullName + "\\" + romanised);
	}
	catch
	{
		log += "ERROR when renaming to: " + romanised;
	}
	log += file + " => " + romanised + "\n";
}

string Romanise(string input)
{
	var output = "";
	var japBuffer = "";

	foreach (var c in input)
	{
		//https://www.ling.upenn.edu/courses/Spring_2003/ling538/UnicodeRanges.html
		if (c >= 0x2E80 && c <= 0x9FAF)
		{
			japBuffer += c;
			continue;
		}

		if (!string.IsNullOrEmpty(japBuffer))
		{
			output += RomanisePhrase(japBuffer);
			japBuffer = "";
		}
		output += c;
	}
	return output;
}

string RomanisePhrase(string input)
{
	try
	{
		var output = converter.Convert(input, To.Romaji,
			Mode.Spaced, RomajiSystem.Nippon).Result;
		output = output.Replace('\"', '\'').Trim();
		return textInfo.ToTitleCase(output);
	}
	catch
	{
		log += "ERROR when romanising: " + input + "\n";
		return input;
	}
}

void SaveLog()
{
	if (string.IsNullOrEmpty(logPath))
	{
		return;
	}
	if (File.Exists(logPath))
	{
		File.Delete(logPath);
	}
	File.WriteAllText(logPath, log);
}