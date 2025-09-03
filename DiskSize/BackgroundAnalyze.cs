using System.IO;

namespace DiskSize;

public static class BackgroundAnalyze
{

	#region Members

	public static IProgress<Tuple<string, int, FileItem>> progressHandler;

	static int progress;
	static DateTime startTime;
	static DateTime lastStatusUpdateTime = DateTime.MinValue;
	static FileItem rootItem;

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

		WIN32_FIND_DATA findData = new()
		{
			dwFileAttributes = FileAttributes.Directory,
			cFileName = path,
		};

		rootItem = new(path, 1, findData)
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

		IntPtr findHandle = WinApi.FindFirstFile(Path.Combine(path, "*"), out WIN32_FIND_DATA findData);

		string newPath;

		if (findHandle != WinApi.INVALID_HANDLE_VALUE)
		{
			do
			{
				if (findData.cFileName.ToString() != "." && findData.cFileName.ToString() != "..")
				{
					newPath = Path.Combine(path, findData.cFileName.ToString());
					items.Add(new FileItem(newPath, level, findData));
				}
			}
			while (WinApi.FindNextFile(findHandle, out findData));
		}

		return items;
	}

	private static void UpdateStatus(string currentPath, bool finalUpdate = false)
	{
		if (finalUpdate || (DateTime.UtcNow - lastStatusUpdateTime).TotalMilliseconds >= 200)
		{
			long size = 0;
			long fileCount = 0;
			foreach (FileItem child in rootItem.Children)
			{
				size += child.Size;
				fileCount += child.FileCount;
			}
			rootItem.Size = size;
			rootItem.FileCount = fileCount;

			Tuple<string, int, FileItem> status = new(currentPath, progress, rootItem);

			progressHandler.Report(status);

			lastStatusUpdateTime = DateTime.UtcNow;
		}
	}

	#endregion

}
