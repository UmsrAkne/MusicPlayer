namespace MusicPlayer.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Prism.Commands;
    using Prism.Mvvm;

    public class MediaDirectory : BindableBase
    {
        private List<MediaDirectory> childDirectory;
        private bool isExpanded = false;
        private bool isSelected = false;

        public MediaDirectory()
        {
            GetChildsCommand = new DelegateCommand(() => GetChild());
        }

        public bool IsM3U
        {
            get => FileInfo.Extension == ".m3u" || FileInfo.Extension == ".m3u16";
        }

        public string Name
        {
            get
            {
                if (FileInfo == null)
                {
                    return string.Empty;
                }

                var rootDirectory = new DriveInfo(FileInfo.FullName);
                if (rootDirectory.RootDirectory.FullName == FileInfo.FullName)
                {
                    return rootDirectory.Name;
                }
                else
                {
                    return FileInfo.Name;
                }
            }
        }

        public DelegateCommand GetChildsCommand { get; private set; }

        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }

        public bool IsExpanded
        {
            get => isExpanded;
            set => SetProperty(ref isExpanded, value);
        }

        public List<MediaDirectory> ChildDirectory
        {
            get => childDirectory;

            set
            {
                SetProperty(ref childDirectory, value);
                childDirectory = value;
            }
        }

        public FileInfo FileInfo { get; set; }

        /// <summary>
        /// m3uファイルに記載されたファイルのリストを生成して取得します。
        /// </summary>
        /// <returns></returns>
        public List<FileInfo> MakeFileListFromM3U()
        {
            var fileList = new List<FileInfo>();
            string[] fileNames = File.ReadAllLines(FileInfo.FullName);

            // 現状、他に影響が出るかはわからないが、
            // staticの値を変更するので、念の為に処理終了の後でアドレスを復元できるようにする。
            string originalCurrentDirectory = Environment.CurrentDirectory;

            Environment.CurrentDirectory = FileInfo.Directory.FullName;

            //// 上記で CurrentDirectory を変更すると、下で相対パスから FileInfo を生成できるようになる。
            //// FileInfo のインスタンスが新規生成される際、コンストラクタの引数に相対パス(..\)が入力された場合、
            //// 基準となるパスは Environment.CurrentDirectory となる模様。

            foreach (string line in fileNames)
            {
                if (line.Trim().Length > 0)
                {
                    if (line.Trim()[0] == '#')
                    {
                        // 先頭が '#' の行はコメント行のためスキップ
                        continue;
                    }

                    FileInfo f = new FileInfo(line);
                    if (f.Exists)
                    {
                        fileList.Add(new FileInfo(line));
                    }
                }
            }

            Environment.CurrentDirectory = originalCurrentDirectory;

            //// 後に影響が出ると嫌なのでもとに戻しておく。

            return fileList;
        }

        private void GetChild()
        {
            if (!Directory.Exists(FileInfo.FullName))
            {
                return;
            }

            string[] childFileNames = Directory.GetDirectories(FileInfo.FullName);
            string[] m3uFileNames = Directory.GetFiles(FileInfo.FullName, "*.m3u");

            var mediaDirectories = new List<MediaDirectory>();
            void addFiles(string[] fileOrDirectoryNames)
            {
                foreach (string n in fileOrDirectoryNames)
                {
                    var md = new MediaDirectory();
                    md.FileInfo = new FileInfo(n);
                    md.GetChild();
                    mediaDirectories.Add(md);
                }
            }

            addFiles(childFileNames);
            addFiles(m3uFileNames);

            ChildDirectory = mediaDirectories;
            Properties.Settings.Default.lastVisitedDirectoryPath = FileInfo.FullName;
            Properties.Settings.Default.Save();
        }
    }
}
