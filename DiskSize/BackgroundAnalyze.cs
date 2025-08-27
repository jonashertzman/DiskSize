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

	public static Tuple<FileItem, TimeSpan> Analyze(string path)
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
			IsExpanded = true
		};

		AnalyzeDirectory(path, rootItem.Children, 2);

		long size = 0;
		foreach (FileItem child in rootItem.Children)
		{
			size += child.Size;
			child.Parent = rootItem;
		}
		rootItem.Size = size;

		return new Tuple<FileItem, TimeSpan>(rootItem, DateTime.UtcNow.Subtract(startTime));
	}

	private static void AnalyzeDirectory(string path, List<FileItem> items, int level)
	{
		if (AnalyzeCancelled) return;
		if (path?.Length > 259) return;
		if (Directory.Exists(path) && !Utils.DirectoryAllowed(path)) return;

		if (level == 3)
		{
			progress++;
		}

		UpdateStatus(path);

		foreach (FileItem fileItem in SearchDirectory(path, level))
		{
			if (AnalyzeCancelled)
			{
				return;
			}

			if (fileItem.IsFolder)
			{
				//fileItem.IsExpanded = true;
				{
					AnalyzeDirectory(Path.Combine(path, fileItem.Name), fileItem.Children, level + 1);

					long size = 0;
					foreach (FileItem child in fileItem.Children)
					{
						size += child.Size;
						child.Parent = fileItem;
					}
					fileItem.Size = size;
				}
			}

			items.Add(fileItem);
		}
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
