using MusicPlayer.model;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer {
    class MainWindowViewModel : BindableBase{

        private List<MediaDirectory> directory = new List<MediaDirectory>();

        public List<MediaDirectory> Directory {
            get {
                return directory;
            }
            private set {
                SetProperty(ref directory, value);
                directory = value;
            }
        }

        private List<FileInfo> mediaFiles;
        public List<FileInfo> MediaFiles {
            get {
                return mediaFiles;
            }
            private set {
                SetProperty(ref mediaFiles, value);
                mediaFiles = value;
            }
        }

        private DelegateCommand<Object> mediaFilesSettingCommand;
        public DelegateCommand<Object> MediaFilesSettingCommand {
            get { return mediaFilesSettingCommand; }
            private set { mediaFilesSettingCommand = value; }
        }

        public MainWindowViewModel() {
            var baseDirectory = new MediaDirectory();
            baseDirectory.FileInfo = new FileInfo(@"C:\Users");

            var dir = new List<MediaDirectory>();
            dir.Add(baseDirectory);
            Directory = dir;

            mediaFilesSettingCommand = new DelegateCommand<Object>(
                (Object param) => {
                    MediaDirectory info = (MediaDirectory)param;
                    string[] fileNames = System.IO.Directory.GetFiles(info.FileInfo.FullName);
                    MediaFiles = new List<FileInfo>();
                    foreach(string n in fileNames) {
                        MediaFiles.Add(new FileInfo(n));
                    }
                },
                (Object param) => { return true; }
            );
        }
    }
}
