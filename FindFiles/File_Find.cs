using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;
using System.IO;

namespace FindFiles
{
	// ファイルに読み書きを行うパラメータ
	//
	[DataContract]
	public class File_Find
	{
		[DataMember]
		public string			 Text	 { get; set; }
		[DataMember]
		public int				 Count	 { get; set; }
	}

	[DataContract]
	public class File_Finds
	{
		[DataMember]
		public List<File_Find>	 Finds	 { get; set; }


		public File_Finds()
		{
			Finds = new List<File_Find>();
		}

		public void Add(string text)
		{
			var ff = Finds.Where(d => d.Text == text).Select(d => d);

			if (ff == null || ff.Count() == 0)
			{
				var f = new File_Find();

				f.Text = text;
				f.Count = 1;

				Finds.Add(f);
			}
			else
			{
				foreach (var f in ff)
				{
					f.Count++;
				}
			}
		}

		static public File_Finds FileLoad(string file_path)
		{
			if (File.Exists(file_path))
			{
				using (var fs = new FileStream(file_path, FileMode.Open))
				{
					var serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(File_Finds));

					File_Finds file = (File_Finds)serializer.ReadObject(fs);

					return file;
				}
			}

			return new File_Finds();
		}

		static public void FileSave(string file_path, File_Finds file)
		{
			var serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(File_Finds));

			using (var fs = new FileStream(file_path, FileMode.Create))
			{
				serializer.WriteObject(fs, file);
			}
		}
	}
}
