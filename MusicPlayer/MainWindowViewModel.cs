using MusicPlayer.model;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
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
        private DoubleSoundPlayer doubleSoundPlayer;

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

        private List<FileInfo> mediaFiles = new List<FileInfo>();
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
                Properties.Settings.Default.Volume = value;
                Properties.Settings.Default.Save();
            }
        }

        public int WindowWidth {
            get => Properties.Settings.Default.WindowWidth;
            set {
                RaisePropertyChanged();
                Properties.Settings.Default.WindowWidth = value;
                Properties.Settings.Default.Save();
            }
        }

        public int WindowHeight {
            get => Properties.Settings.Default.WindowHeight;
            set {
                RaisePropertyChanged();
                Properties.Settings.Default.WindowHeight = value;
                Properties.Settings.Default.Save();
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

        private IDialogService dialogService;

        private PlayerSetting playerSetting;

        private DelegateCommand showSettingDialogCommand;
        public DelegateCommand ShowSettingDialogCommand {
            get => showSettingDialogCommand ?? (showSettingDialogCommand = new DelegateCommand(
                () => {
                    var param = new DialogParameters();
                    param.Add(nameof(PlayerSetting), playerSetting);
                    dialogService.ShowDialog(nameof(SettingWindow), param,
                        (IDialogResult result) => {
                            if (result.Parameters.GetValue<PlayerSetting>(nameof(SettingWindowViewModel.Setting)) == null) {
                                // バツを押して閉じた時とかに値がNullになっている
                                return;
                            }
                            else {
                                PlayerSetting pSettings = result.Parameters.GetValue<PlayerSetting>(nameof(SettingWindowViewModel.Setting));
                                doubleSoundPlayer.SwitchingDuration = pSettings.SwitchingDuration;
                                Properties.Settings.Default.SwitchinDuration = pSettings.SwitchingDuration;
                                Properties.Settings.Default.DefaultBaseDirectoryPath = pSettings.DefaultBaseDirectoryPath;
                                Properties.Settings.Default.Save();
                            }
                        }
                    );
                }
            ));
        }

        public MainWindowViewModel(IDialogService _dialogService) {
            dialogService = _dialogService;

            string lastVisitedDirectoryPath = Properties.Settings.Default.lastVisitedDirectoryPath;
            var path = (new DirectoryInfo(Properties.Settings.Default.DefaultBaseDirectoryPath).Exists) ?
                Properties.Settings.Default.DefaultBaseDirectoryPath : @"C:\";
            BaseDirectoryPath = path;

            doubleSoundPlayer = new DoubleSoundPlayer(
                new SoundPlayer(new WMPWrapper()),
                new SoundPlayer(new WMPWrapper())
            );

            playerSetting = new PlayerSetting();
            playerSetting.DefaultBaseDirectoryPath = path;
            playerSetting.SwitchingDuration = Properties.Settings.Default.SwitchinDuration;
            DoubleSoundPlayer.SwitchingDuration = playerSetting.SwitchingDuration;
            DoubleSoundPlayer.Volume = Properties.Settings.Default.Volume;

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

                    doubleSoundPlayer.Files = mediaFiles;
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

            List<MediaDirectory> mdList = expandItemsTo(lastVisitedDirectoryPath);
            if(mdList.Count != 0) {
                Directory = mdList;
            }

        }

        private DelegateCommand randomSortCommand;
        public DelegateCommand RandomSortCommand {
            get => randomSortCommand ?? (randomSortCommand = new DelegateCommand(
                () => {
                    Random r = new Random();
                    MediaFiles = MediaFiles.OrderBy(m => r.Next(MediaFiles.Count)).ToList();
                },
                () => MediaFiles.Count > 0
            )).ObservesProperty(() => MediaFiles);
        }

        private DelegateCommand nameOrderSortCommand;
        public DelegateCommand NameOrderSortCommand {
            get => nameOrderSortCommand ?? (nameOrderSortCommand = new DelegateCommand(
                () => {
                    if (MediaFiles != null && MediaFiles.Count > 0) {
                        MediaFiles = MediaFiles.OrderBy(m => m.Name).ToList();
                    }

                },
                () => MediaFiles.Count > 0
            )).ObservesProperty(() => MediaFiles);
        }

        /// <summary>
        /// 指定したパスに含まれるディレクトリが展開された状態のリストを生成します。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private List<MediaDirectory> expandItemsTo(string path) {
            if (!System.IO.Directory.Exists(path) || path == baseDirectoryPath) {
                return new List<MediaDirectory>();
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            List<DirectoryInfo> directoryInfoList = new List<DirectoryInfo>();

            while(directoryInfo != null){
                directoryInfoList.Add(directoryInfo);
                directoryInfo = directoryInfo.Parent;
                if(directoryInfo.FullName == new DirectoryInfo(BaseDirectoryPath).Parent.FullName) {
                    break;
                }
            }

            // ルートに近いディレクトリほど奥に入っていて、ループ処理が逆順になってしまうため逆転させる。
            directoryInfoList.Reverse();

            MediaDirectory md = new MediaDirectory();
            List<MediaDirectory> mdList = new List<MediaDirectory>();
            mdList.Add(md);

            for(var i = 0; i < directoryInfoList.Count; i++) {
                md.FileInfo = new FileInfo(directoryInfoList[i].FullName);
                md.GetChildsCommand.Execute();
                md.IsExpanded = true;

                if(directoryInfoList.Count <= i + 1) {
                    break;
                }

                md = md.ChildDirectory.FirstOrDefault(m => m.FileInfo.FullName == directoryInfoList[i + 1].FullName);
            }

            md.IsExpanded = true;
            md.IsSelected = true;
            MediaFilesSettingCommand.Execute(md);
            if(DoubleSoundPlayer.Files != null && DoubleSoundPlayer.Files.Count != 0) {
                int sameFileNameIndex = DoubleSoundPlayer.Files.FindIndex(f => f.FullName == Properties.Settings.Default.lastPlayingFileName);
                DoubleSoundPlayer.PlayingIndex = sameFileNameIndex;
                DoubleSoundPlayer.updateSelectedFileName();
            }

            return mdList;
        }
    }
}
