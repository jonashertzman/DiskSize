using System.Collections.ObjectModel;
using System.IO;

namespace DiskSize;

public class FileItem
{

	#region Constructor

	public FileItem()
	{

	}

	public FileItem(string path, int level, WIN32_FIND_DATA findData)
	{
		Name = findData.cFileName;
		Path = path;
		Level = level;

		IsFolder = (findData.dwFileAttributes & FileAttributes.Directory) != 0;

		if (!IsFolder)
		{
			Size = (long)Combine(findData.nFileSizeHigh, findData.nFileSizeLow);
		}

		Date = DateTime.FromFileTime((long)Combine(findData.ftLastWriteTime.dwHighDateTime, findData.ftLastWriteTime.dwLowDateTime));
		if (level == 1)
		{
			Date = null;
		}
	}

	#endregion

	#region Overrides

	public override string ToString()
	{
		return $"{Name}";
	}

	#endregion

	#region Properties

	public FileItem Parent { get; set; }

	public ObservableCollection<FileItem> Children { get; set; } = [];

	public string Path { get; set; } = "";

	public string Name { get; set; } = "";

	public DateTime? Date { get; set; }

	public long Size { get; set; }

	public int Level { get; set; }

	public bool IsFolder { get; set; }

	private bool isExpanded;
	public bool IsExpanded
	{
		get { return this.isExpanded; }
		set
		{
			if (value != this.isExpanded)
			{
				this.isExpanded = value;
			}
		}
	}

	#endregion

	#region Methods

	private ulong Combine(uint highValue, uint lowValue)
	{
		return (ulong)highValue << 32 | lowValue;
	}

	#endregion

}
