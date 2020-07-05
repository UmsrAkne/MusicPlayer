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

        public String Name {
            get {
                if (FileInfo == null) return "";
                else return FileInfo.Name;
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
            var mediaDirectories = new List<MediaDirectory>();
            string[] childFileNames = Directory.GetDirectories(FileInfo.FullName);
            string[] m3uFileNames = Directory.GetFiles(FileInfo.FullName, "*.m3u");

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
        }

        public DelegateCommand GetChildsCommand { get; private set; }
    }
}
