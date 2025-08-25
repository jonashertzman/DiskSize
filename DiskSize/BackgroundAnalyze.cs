using System.Collections.ObjectModel;
using System.IO;

namespace DiskSize;

public static class BackgroundAnalyze
{

	#region Members

	internal static IProgress<Tuple<string, int>> progressHandler;

	private static int progress;
	private static DateTime startTime;

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

		WIN32_FIND_DATA findData = new()
		{
			dwFileAttributes = FileAttributes.Directory,
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

	private static void AnalyzeDirectory(string path, ObservableCollection<FileItem> items, int level)
	{
		if (AnalyzeCancelled) return;
		if (path?.Length > 259) return;
		if (Directory.Exists(path) && !Utils.DirectoryAllowed(path)) return;

		if (level == 3)
		{
			progress++;
			Debug.WriteLine(progress);
		}

		foreach (FileItem fileItem in SearchDirectory(path, level))
		{
			if (AnalyzeCancelled)
			{
				return;
			}

			UpdateStatus(fileItem.Path);

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

	public static List<FileItem> SearchDirectory(string path, int level)
	{
		path = Utils.FixRootPath(path);
		List<FileItem> items = [];

		IntPtr findHandle = WinApi.FindFirstFile(Path.Combine(path, "*"), out WIN32_FIND_DATA findData);

		string newPath;

		if (findHandle != WinApi.INVALID_HANDLE_VALUE)
		{
			do
			{
				if (findData.cFileName != "." && findData.cFileName != "..")
				{
					newPath = Path.Combine(path, findData.cFileName);
					items.Add(new FileItem(newPath, level, findData));
				}
			}
			while (WinApi.FindNextFile(findHandle, out findData));
		}

		WinApi.FindClose(findHandle);

		return items;
	}

	static DateTime lastStatusUpdateTime = DateTime.MinValue;

	private static void UpdateStatus(string currentPath, bool finalUpdate = false)
	{
		if (finalUpdate || (DateTime.UtcNow - lastStatusUpdateTime).TotalMilliseconds >= 200)
		{
			Tuple<string, int> tuple = new(currentPath, progress);

			progressHandler.Report(tuple);

			lastStatusUpdateTime = DateTime.UtcNow;
		}
	}

	#endregion

}
