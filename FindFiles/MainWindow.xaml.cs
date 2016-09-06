using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO;
using Microsoft.Win32;
using win = System.Windows.Forms;

namespace FindFiles
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		private List<FileInfo> filesList = new List<FileInfo>();

		private File_Finds file_Finds = null;


		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			// 設定から読み取る。
			this.Left = Properties.Settings.Default.WindowLeft;
			this.Top = Properties.Settings.Default.WindowTop;
			this.Width = Properties.Settings.Default.WindowWidth;
			this.Height = Properties.Settings.Default.WindowHeight;

			this.label_FindFolder.Content = Properties.Settings.Default.FindFolder;
			this.comboBox_FindFile.Text = Properties.Settings.Default.FindFile;

			file_Finds = File_Finds.FileLoad("FindFiles.xml");

			GetFileFindsSort();

			// FindFolderが何も無かったら、
			if (label_FindFolder.Content.Equals(""))
			{
				// FindFolderに初期値を送る
				label_FindFolder.Content = "C:\\";
			}

			if (comboBox_FindFile.Text.Equals(""))
			{
				button_FileFind.IsEnabled = false;
			}
			else
			{
				button_FileFind.IsEnabled = true;
			}

			button_Copy.IsEnabled = false;
			button_AddressCopy.IsEnabled = false;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (this.WindowState == WindowState.Normal)
			{
				// 設定に書き込む。
				Properties.Settings.Default.WindowLeft = this.Left;
				Properties.Settings.Default.WindowTop = this.Top;
				Properties.Settings.Default.WindowWidth = this.Width;
				Properties.Settings.Default.WindowHeight = this.Height;
			}

			// 設定にフォルダ名を書き込む。
			Properties.Settings.Default.FindFolder = (string)this.label_FindFolder.Content;
			Properties.Settings.Default.FindFile = this.comboBox_FindFile.Text;

			// 設定に送ったものをセーブ。
			Properties.Settings.Default.Save();
	
			// 設定ファイルを、指定した文字列にセーブする
			File_Finds.FileSave("FindFiles.xml", file_Finds);
	}

		private void button_FindFolder_Click(object sender, RoutedEventArgs e)
		{
			{
				// フォルダブラウザーダイアログ
				win.FolderBrowserDialog fbd = new win.FolderBrowserDialog();

				fbd.Description = "検索するフォルダを指定してください。";

				fbd.RootFolder = Environment.SpecialFolder.Desktop;

				fbd.SelectedPath = (string)label_FindFolder.Content;

				fbd.ShowNewFolderButton = true;

				win.DialogResult result = fbd.ShowDialog();

				if (result == win.DialogResult.OK)
				{
					label_FindFolder.Content = fbd.SelectedPath;
				}
			}
		}

		private void button_FileFind_Click(object sender, RoutedEventArgs e)
		{
			file_Finds.Add(comboBox_FindFile.Text);

			GetFileFindsSort();

			ToFindFolder();
		}

		private void textBox_FindFile_KeyDown(object sender, KeyEventArgs e)
		{
		}

		private void textBox_FindFile_KeyUp(object sender, KeyEventArgs e)
		{
		}

		private void comboBox_FindFile_KeyUp(object sender, KeyEventArgs e)
		{
			if (comboBox_FindFile.Text.Equals(""))
			{
				button_FileFind.IsEnabled = false;
			}
			else
			{
				button_FileFind.IsEnabled = true;
			}

			if (button_FileFind.IsEnabled)
			{
				// キーUPのKeyがEnterの時
				if (e.Key == Key.Enter)
				{
					file_Finds.Add(comboBox_FindFile.Text);

					GetFileFindsSort();

					ToFindFolder();
				}
			}
		}

		private void listBox_Find_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (listBox_Find.SelectedItems.Count > 0)
			{
				label_FindCount.Content = filesList.Count + " of " + listBox_Find.SelectedItems.Count;
			}
			else
			{
				label_FindCount.Content = filesList.Count;
			}

			if (listBox_Find.Items.Count == 0)
			{
				button_Copy.IsEnabled		 = false;
				button_AddressCopy.IsEnabled = false;
			}
			else if (
				listBox_Find.Items.Count == 1)
			{
				button_Copy.IsEnabled		 = true;
				button_AddressCopy.IsEnabled = true;
			}
			else if (
				listBox_Find.Items.Count > 1)
			{
				if (listBox_Find.SelectedItems.Count == 0)
				{
					button_Copy.IsEnabled		 = false;
					button_AddressCopy.IsEnabled = false;
				}
				else
				{
					button_Copy.IsEnabled		 = true;
					button_AddressCopy.IsEnabled = true;
				}
			}
		}

		private void listBox_Find_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			// listBox は ListBox のインスタンス
			object item = listBox_Find.SelectedItem;
			if (item == null)
			{
				// 選択されてない = ダブルクリックもされてないと考える
				return;
			}
			UIElement element = (UIElement)listBox_Find.ItemContainerGenerator.ContainerFromItem(item);
			if (element.InputHitTest(e.GetPosition(element)) == null)
			{
				// ダブルクリックされた IInputElement がない = 別の場所がダブルクリックされた
				return;
			}
			// item がダブルクリックされた

			Files f = (Files)item;

			System.Diagnostics.Process.Start(f.Path);
		}

		private void button_Show_Click(object sender, RoutedEventArgs e)
		{
			var files = new System.Collections.Specialized.StringCollection();

			if (listBox_Find.Items.Count == 1)
			{
				files.Add(filesList[0].FullName);
			}
			else if (
				listBox_Find.SelectedItems.Count > 0)
			{
				foreach (var item in listBox_Find.SelectedItems)
				{
					files.Add(((Files)item).Path);
				}
			}

			Clipboard.SetFileDropList(files);
		}

		private void button_Copy_Click(object sender, RoutedEventArgs e)
		{
			var files = new System.Collections.Specialized.StringCollection();

			if (listBox_Find.Items.Count == 1)
			{
				files.Add(filesList[0].FullName);
			}
			else if (
				listBox_Find.SelectedItems.Count > 0)
			{
				foreach (var item in listBox_Find.SelectedItems)
				{
					files.Add(((Files)item).Path);
				}
			}

			Clipboard.SetFileDropList(files);
		}

		private void button_AddressCopy_Click(object sender, RoutedEventArgs e)
		{
			if (listBox_Find.Items.Count == 1)
			{
				Clipboard.SetText(filesList[0].FullName);
			}
			else if (
				listBox_Find.SelectedItems.Count > 0)
			{
				var text = "";

				foreach (var item in listBox_Find.SelectedItems)
				{
					text += ((Files)item).Path + @"\";
				}

				Clipboard.SetText(text);
			}
		}

		private void ToFindFolder()
		{
			listBox_Find.Items.Clear();
		
			filesList = new List<FileInfo>();

			FindFolder(new DirectoryInfo((string)label_FindFolder.Content));

			label_FindCount.Content = filesList.Count;

			if (listBox_Find.Items.Count == 0)
			{
				button_Copy.IsEnabled		 = false;
				button_AddressCopy.IsEnabled = false;
			}
			else if (
				listBox_Find.Items.Count == 1)
			{
				button_Copy.IsEnabled		 = true;
				button_AddressCopy.IsEnabled = true;
			}
			else if (
				listBox_Find.Items.Count > 1)
			{
				button_Copy.IsEnabled		 = false;
				button_AddressCopy.IsEnabled = false;
			}
		}

		private void FindFolder(DirectoryInfo di)
		{
			var text = comboBox_FindFile.Text;

			try
			{
				foreach (var fi in di.GetFiles())
				{
					if (fi.Name.StartsWith(text))
					{
					}

					var index = fi.Name.IndexOf(text);
					if (index > -1)
					{
						Console.WriteLine(fi.Name);

						filesList.Add(fi);

						var files_ = new Files();

						if (fi.Extension.ToLower().Equals(".bmp"	) ||
							fi.Extension.ToLower().Equals(".gif"	) ||
							fi.Extension.ToLower().Equals(".jpg"	) ||
							fi.Extension.ToLower().Equals(".jpeg"	) ||
							fi.Extension.ToLower().Equals(".png"	) ||
							fi.Extension.ToLower().Equals(".tiff"	) ||
							fi.Extension.ToLower().Equals(".ico"	))
						{
							files_.Icon = new BitmapImage(new Uri(fi.FullName));
						}
						else
						{
							files_.Icon = SHFileInfo.GetBitmap(fi.FullName);
						}

						files_.File	 = fi.Name;
						files_.Path	 = fi.FullName;
						files_.Size	 = fi.Length;
						files_.SizeS = ToString(fi.Length);

						listBox_Find.Items.Add(files_);
					}
				}

				foreach (var cdi in di.GetDirectories())
				{
					FindFolder(cdi);
				}
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		private void button_List_1_Click(object sender, RoutedEventArgs e)
		{

		}

		private void button_List_2_Click(object sender, RoutedEventArgs e)
		{

		}

		private void button_List_3_Click(object sender, RoutedEventArgs e)
		{

		}

		private string ToString(long length)
		{
			if (length < 1000)
			{
				return length.ToString("#,0 B");
			}

			length /= 1000;

			if (length < 1000)
			{
				return length.ToString("#,0 KB");
			}

			length /= 1000;

			if (length < 1000)
			{
				return length.ToString("#,0 MB");
			}

			length /= 1000;

			if (length < 1000)
			{
				return length.ToString("#,0 GB");
			}

			length /= 1000;

			return length.ToString("#,0 TB");
		}

		private void GetFileFindsSort()
		{
			var ff = file_Finds.Finds.OrderByDescending(f => f.Count);

			comboBox_FindFile.ItemsSource = ff;
		}
	}

    public class Files
	{
		public BitmapImage	 Icon	 { get; set; }
		public string		 File	 { get; set; }
		public string		 Path	 { get; set; }
		public long			 Size	 { get; set; }
		public string		 SizeS	 { get; set; }
	}
}

