using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;

namespace MusicPlayer.model {
    class MediaDirectory {

        public String Name {
            get {
                if (FileInfo == null) return "";
                else return FileInfo.Name;
            }
        }

        public List<MediaDirectory> ChildDirectory { get; set; }
        public FileInfo FileInfo { get; set; }

        public MediaDirectory() {
            GetChildsCommand = new DelegateCommand(
                () => { getChild(); },
                () => { return true; }
            );
        }

        private void getChild() {
            ChildDirectory = new List<MediaDirectory>();
            string[] childFileNames = System.IO.Directory.GetDirectories(FileInfo.FullName);
            foreach (string n in childFileNames) {
                var md = new MediaDirectory();
                md.FileInfo = new FileInfo(n);
                ChildDirectory.Add(md);
            }
        }

        public DelegateCommand GetChildsCommand { get; private set; }
    }
}
