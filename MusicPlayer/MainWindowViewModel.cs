using MusicPlayer.model;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer {
    class MainWindowViewModel : BindableBase{

        private List<MediaDirectory> directory = new List<MediaDirectory>();
        private DoubleSoundPlayer doubleSoundPlayer = new DoubleSoundPlayer();

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

        public bool Playing {
            get {
                return doubleSoundPlayer.Playing;
            }
            private set {
                RaisePropertyChanged(nameof(Playing));
            }
        }

        private DelegateCommand<Object> mediaFilesSettingCommand;
        public DelegateCommand<Object> MediaFilesSettingCommand {
            get { return mediaFilesSettingCommand; }
            private set { mediaFilesSettingCommand = value; }
        }

        public DelegateCommand PlayCommand { get; private set; }
        public DelegateCommand StopCommand { get; private set; }

        public MainWindowViewModel() {
            var baseDirectory = new MediaDirectory();
            baseDirectory.FileInfo = new FileInfo(@"C:\Users");
            doubleSoundPlayer.SwitchingDuration = 10;

            var dir = new List<MediaDirectory>();
            dir.Add(baseDirectory);
            Directory = dir;

            mediaFilesSettingCommand = new DelegateCommand<Object>(
                (Object param) => {
                    MediaDirectory info = (MediaDirectory)param;

                    string[] fileNames = System.IO.Directory.GetFiles(info.FileInfo.FullName);
                    IEnumerable<String> selectedList = from name in fileNames
                                                       where name.EndsWith(".mp3")
                                                       select name;

                    MediaFiles = new List<FileInfo>();
                    foreach(string n in selectedList) {
                        MediaFiles.Add(new FileInfo(n));
                    }
                },
                (Object param) => { return true; }
            );

            PlayCommand = new DelegateCommand(
                () => {
                    doubleSoundPlayer.Files = MediaFiles;
                    doubleSoundPlayer.play();
                    Playing = true;
                },
                () => { return MediaFiles != null && MediaFiles.Count > 0; }
            ).ObservesProperty(() => MediaFiles );

            StopCommand = new DelegateCommand(
                () => {
                    doubleSoundPlayer.stop();
                    Playing = false;
                },
                () => { return doubleSoundPlayer.Playing; }
            ).ObservesProperty(() => Playing );
        }
    }
}
