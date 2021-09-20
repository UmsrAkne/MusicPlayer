namespace MusicPlayer.Models
{
    using System;
    using System.IO;

    public class IndexedFileInfo
    {
        private FileInfo fileInfo;

        public IndexedFileInfo(FileInfo f) => fileInfo = f;

        public int Index { get; set; }

        public FileInfo FileInfo => fileInfo;

        public string Name => fileInfo.Name;

        public string FullName => fileInfo.FullName;

        public string SizeOfMB => Math.Round((double)FileInfo.Length / 1024 / 1024, 2).ToString() + " MB";
    }
}
