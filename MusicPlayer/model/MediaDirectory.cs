using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;

namespace MusicPlayer.model {
    class MediaDirectory : BindableBase{

        public bool IsM3U {
            get {
                return FileInfo.Extension == ".m3u";
            }
        }

        public String Name {
            get {
                if (FileInfo == null) {
                    return "";
                }

                var rootDirectory = new DriveInfo(FileInfo.FullName);
                if (rootDirectory.RootDirectory.FullName == FileInfo.FullName) {
                    return rootDirectory.Name;
                }
                else {
                    return FileInfo.Name;
                }
            }
        }

        private List<MediaDirectory> childDirectory;
        public List<MediaDirectory> ChildDirectory {
            get {
                return childDirectory;
            }
            set {
                SetProperty(ref childDirectory, value);
                    childDirectory = value; 
            }
        }
        public FileInfo FileInfo { get; set; }

        public MediaDirectory() {
            GetChildsCommand = new DelegateCommand(
                () => { getChild(); },
                () => { return true; }
            );
        }

        private void getChild() {
            if (!Directory.Exists(FileInfo.FullName)) {
                return;
            }

            string[] childFileNames = Directory.GetDirectories(FileInfo.FullName);
            string[] m3uFileNames = Directory.GetFiles(FileInfo.FullName, "*.m3u");

            var mediaDirectories = new List<MediaDirectory>();
            void addFiles(string[] fileOrDirectoryNames) {
                 foreach(string n in fileOrDirectoryNames) {
                     var md = new MediaDirectory();
                     md.FileInfo = new FileInfo(n);
                     mediaDirectories.Add(md);
                 }
            }

            addFiles(childFileNames);
            addFiles(m3uFileNames);

            ChildDirectory = mediaDirectories;
            Properties.Settings.Default.lastVisitedDirectoryPath = FileInfo.FullName;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// m3uファイルに記載されたファイルのリストを生成して取得します。
        /// </summary>
        /// <returns></returns>
        public List<FileInfo> makeFileListFromM3U() {
            var fileList = new List<FileInfo>();
            string[] fileNames = File.ReadAllLines(FileInfo.FullName);
            foreach(var n in fileNames) {
                if(n.Trim().Length > 0) {
                    FileInfo f = new FileInfo(n);
                    if (f.Exists) {
                        fileList.Add(new FileInfo(n));
                    }
                }
            }

            return fileList;
        }

        public DelegateCommand GetChildsCommand { get; private set; }

        private bool isExpanded = false;
        public bool IsExpanded {
            get => isExpanded;
            set => SetProperty(ref isExpanded, value);
        }

        private bool isSelected = false;
        public bool IsSelected {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }
    }
}
