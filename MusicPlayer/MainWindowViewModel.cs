using MusicPlayer.model;
using MusicPlayer.viewModels;
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

        public TreeViewModel TreeViewModel {
            get;
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

        private DelegateCommand<Object> mediaFilesSettingCommand;
        public DelegateCommand<Object> MediaFilesSettingCommand {
            get { return mediaFilesSettingCommand; }
            private set { mediaFilesSettingCommand = value; }
        }

        public String SelectedDirectoryName { get; private set; }

        public DelegateCommand PlayCommand { get; private set; }

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

            TreeViewModel = new TreeViewModel(path);
            TreeViewModel.expandItemsTo(lastVisitedDirectoryPath);

            doubleSoundPlayer = new DoubleSoundPlayer(
                new SoundPlayer(new WMPWrapper()),
                new SoundPlayer(new WMPWrapper())
            );

            doubleSoundPlayer.CurrentDirectorySource = TreeViewModel;

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

            PlayCommand = new DelegateCommand(
                () => {
                    doubleSoundPlayer.Files = MediaFiles;
                    doubleSoundPlayer.play();
                },
                () => { return MediaFiles != null && MediaFiles.Count > 0; }
            ).ObservesProperty(() => MediaFiles );

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

    }
}
