using System.IO;
using Windows.Win32.Storage.FileSystem;

namespace DiskSize;

public static class BackgroundAnalyze
{

	#region Members

	public static IProgress<Tuple<string, int>> progressHandler;

	static int progress;
	static DateTime startTime;
	static DateTime lastStatusUpdateTime = DateTime.MinValue;

	#endregion

	#region Properties

	public static bool AnalyzeCancelled { get; private set; } = false;

	#endregion

	#region Methods

	public static void Cancel()
	{
		AnalyzeCancelled = true;
	}

	public static Tuple<FileItem, TimeSpan, bool> Analyze(string path)
	{
		path = Path.TrimEndingDirectorySeparator(path);

		startTime = DateTime.UtcNow;

		AnalyzeCancelled = false;
		progress = 0;

		WIN32_FIND_DATAW findData = new()
		{
			dwFileAttributes = (uint)FileAttributes.Directory,
			cFileName = path,
		};

		FileItem rootItem = new(path, 1, findData)
		{
			IsExpanded = true,
			Date = Directory.GetLastWriteTime(path),
		};

		AnalyzeDirectory(rootItem, 2);

		return new Tuple<FileItem, TimeSpan, bool>(rootItem, DateTime.UtcNow.Subtract(startTime), AnalyzeCancelled);
	}

	private static void AnalyzeDirectory(FileItem item, int level)
	{
		if (item.Path?.Length > 259) return;

		if (!Utils.DirectoryAllowed(item.Path))
		{
			item.Unauthorized = true;
			return;
		}

		if (level == 3)
		{
			progress++;
		}

		UpdateStatus(item.Path);

		foreach (FileItem fileItem in SearchDirectory(item.Path, level))
		{
			if (AnalyzeCancelled)
			{
				break;
			}

			if (fileItem.IsFolder)
			{
				//fileItem.IsExpanded = true;
				{
					AnalyzeDirectory(fileItem, level + 1);
				}
			}

			item.Children.Add(fileItem);
		}

		long size = 0;
		long fileCount = 0;
		foreach (FileItem child in item.Children)
		{
			size += child.Size;
			fileCount += child.FileCount;
			child.Parent = item;
		}
		item.Size = size;
		item.FileCount = fileCount;
	}

	private static List<FileItem> SearchDirectory(string path, int level)
	{
		path = Utils.FixRootPath(path);
		List<FileItem> items = [];

		using var findHandle = PInvoke.FindFirstFile(Path.Combine(path, "*"), out WIN32_FIND_DATAW findData);

		string newPath;

		if (!findHandle.IsInvalid)
		{
			do
			{
				if (findData.cFileName.ToString() != "." && findData.cFileName.ToString() != "..")
				{
					newPath = Path.Combine(path, findData.cFileName.ToString());
					items.Add(new FileItem(newPath, level, findData));
				}
			}
			while (PInvoke.FindNextFile(findHandle, out findData));
		}

		return items;
	}

	private static void UpdateStatus(string currentPath, bool finalUpdate = false)
	{
		if (finalUpdate || (DateTime.UtcNow - lastStatusUpdateTime).TotalMilliseconds >= 200)
		{
			Tuple<string, int> status = new(currentPath, progress);

			progressHandler.Report(status);

			lastStatusUpdateTime = DateTime.UtcNow;
		}
	}

	#endregion

}
