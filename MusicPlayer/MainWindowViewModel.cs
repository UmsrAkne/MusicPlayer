﻿using MusicPlayer.model;
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

        public DoubleSoundPlayer DoubleSoundPlayer {
            get { return doubleSoundPlayer; }
        }

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

        public int Volume {
            get {
                return doubleSoundPlayer.Volume;
            }
            set {
                RaisePropertyChanged(nameof(Volume));
                doubleSoundPlayer.Volume = value;
            }
        }

        private string baseDirectoryPath = "";
        public string BaseDirectoryPath {
            get => baseDirectoryPath;
            set {
                if (!System.IO.Directory.Exists(value)) {
                    return;
                }

                SetProperty(ref baseDirectoryPath, value);
                baseDirectoryPath = value;

                var md = new MediaDirectory();
                md.FileInfo = new FileInfo(value);
                md.GetChildsCommand.Execute();

                var dir = new List<MediaDirectory>();
                dir.Add(md);
                Directory = dir;
            }
        }

        private DelegateCommand<Object> mediaFilesSettingCommand;
        public DelegateCommand<Object> MediaFilesSettingCommand {
            get { return mediaFilesSettingCommand; }
            private set { mediaFilesSettingCommand = value; }
        }

        public DelegateCommand<Object> SelectedDirectorySettingCommand { get; private set; }
        public String SelectedDirectoryName { get; private set; }

        public DelegateCommand PlayCommand { get; private set; }
        public DelegateCommand StopCommand { get; private set; }

        public MainWindowViewModel() {

            BaseDirectoryPath = (@"C:\");

            mediaFilesSettingCommand = new DelegateCommand<Object>(
                (Object param) => {
                    MediaDirectory info = (MediaDirectory)param;

                    MediaFiles = new List<FileInfo>();

                    if (info.IsM3U) {
                        var fileList = info.makeFileListFromM3U();
                        foreach(FileInfo f in fileList) {
                            mediaFiles.Add(f);
                        }
                    }
                    else {
                        string[] fileNames = System.IO.Directory.GetFiles(info.FileInfo.FullName);
                        IEnumerable<String> selectedList = from name in fileNames
                                                           where name.EndsWith(".mp3")
                                                           select name;

                        foreach(string n in selectedList) {
                            MediaFiles.Add(new FileInfo(n));
                        }
                    }

                },
                (Object param) => { return true; }
            );

            SelectedDirectorySettingCommand = new DelegateCommand<object>(
                (Object param) => {
                    SelectedDirectoryName = ((MediaDirectory)param).FileInfo.FullName;
                    RaisePropertyChanged(nameof(SelectedDirectoryName));
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

        private DelegateCommand randomSortCommand;
        public DelegateCommand RandomSortCommand {
            get => randomSortCommand ?? (randomSortCommand = new DelegateCommand(
                () => {
                    if(MediaFiles.Count == 0) {
                        return;
                    }

                    Random r = new Random();
                    MediaFiles = MediaFiles.OrderBy(m => r.Next(MediaFiles.Count)).ToList();
                }
            ));
        }

        private DelegateCommand nameOrderSortCommand;
        public DelegateCommand NameOrderSortCommand {
            get => nameOrderSortCommand ?? (nameOrderSortCommand = new DelegateCommand(
                () => {
                    MediaFiles = MediaFiles.OrderBy(m => m.Name).ToList();
                }
            ));
        }
    }
}
