using System.Windows;

namespace DiskSize;

public partial class MsgBox : Window
{

	public MsgBox()
	{
		InitializeComponent();

		DataContext = this;
	}

	public string Text { get; set; }

	public string Caption { get; set; }

	public static void Show(string text, string title)
	{
		MsgBox msgBox = new()
		{
			Owner = Application.Current.MainWindow,
			Text = text,
			Caption = title
		};

		msgBox.ShowDialog();
	}

}
