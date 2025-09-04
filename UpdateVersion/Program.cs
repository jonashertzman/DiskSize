using System.IO.Compression;

namespace UpdateVersion;

class Program
{
	static void Main()
	{
		DateTime buildDate = DateTime.Now;
		string buildNumber = $"{buildDate:yy}{buildDate.DayOfYear:D3}";

		Console.WriteLine($"Updating version to {buildNumber}");

		File.WriteAllText(@"..\docs\download\version.txt", buildNumber);


		Console.WriteLine($"Updating download");

		File.Delete(@"..\docs\download\DiskSize.zip");

		using ZipArchive download = ZipFile.Open(@"..\docs\download\DiskSize.zip", ZipArchiveMode.Create);
		download.CreateEntryFromFile(@".\bin\Publish\DiskSize.exe", "DiskSize.exe");
		download.CreateEntryFromFile(@"..\LICENSE.txt", "LICENSE.txt");
	}
}
