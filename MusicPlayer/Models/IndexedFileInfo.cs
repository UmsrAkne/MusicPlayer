namespace MusicPlayer.Models
{
    using Prism.Mvvm;
    using System;
    using System.IO;

    public class IndexedFileInfo : BindableBase
    {
        private FileInfo fileInfo;
        private bool isSelected;

        public IndexedFileInfo(FileInfo f) => fileInfo = f;

        public int Index { get; set; }

        public FileInfo FileInfo => fileInfo;

        public string Name => fileInfo.Name;

        public string FullName => fileInfo.FullName;

        public string SizeOfMB => Math.Round((double)FileInfo.Length / 1024 / 1024, 2).ToString() + " MB";

        public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value); }
    }
}
