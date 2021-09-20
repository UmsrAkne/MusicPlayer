using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.Models
{
    public class IndexedFileInfo
    {
        public int Index { get; set; }
        private FileInfo fileInfo;

        public IndexedFileInfo(FileInfo f)
        {
            fileInfo = f;
        }

        public FileInfo FileInfo
        {
            get => fileInfo;
        }

        public String Name
        {
            get => fileInfo.Name;
        }

        public String FullName
        {
            get => fileInfo.FullName;
        }

        public string SizeOfMB
        {
            get => Math.Round((double)FileInfo.Length / 1024 / 1024, 2).ToString() + " MB";
        }

    }
}
