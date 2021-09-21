using MusicPlayer.Models;
using MusicPlayer.views;
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

namespace MusicPlayer.ViewModels
{
    class MainWindowViewModel : BindableBase
    {

        private DoubleSoundPlayer doubleSoundPlayer;

        public DoubleSoundPlayer DoubleSoundPlayer
        {
            get { return doubleSoundPlayer; }
        }

        public TreeViewModel TreeViewModel
        {
            get;
        }

        private List<IndexedFileInfo> mediaFiles = new List<IndexedFileInfo>();
        public List<IndexedFileInfo> MediaFiles
        {
            get
            {
                return mediaFiles;
            }
            private set
            {

                for (int i = 0; i < value.Count; i++)
                {
                    value[i].Index = i + 1; // 表示番号は１始まりとしたいので +1
                }

                SetProperty(ref mediaFiles, value);
            }
        }

        public int WindowWidth
        {
            get => Properties.Settings.Default.WindowWidth;
            set
            {
                RaisePropertyChanged();
                Properties.Settings.Default.WindowWidth = value;
                Properties.Settings.Default.Save();
            }
        }

        public int WindowHeight
        {
            get => Properties.Settings.Default.WindowHeight;
            set
            {
                RaisePropertyChanged();
                Properties.Settings.Default.WindowHeight = value;
                Properties.Settings.Default.Save();
            }
        }

        private DelegateCommand<Object> mediaFilesSettingCommand;
        public DelegateCommand<Object> MediaFilesSettingCommand
        {
            get => mediaFilesSettingCommand ?? (mediaFilesSettingCommand = new DelegateCommand<object>(
                (object param) =>
                {
                    MediaDirectory info = (MediaDirectory)param;
                    MediaFiles = new List<IndexedFileInfo>();
                    List<IndexedFileInfo> mf = new List<IndexedFileInfo>();

                    if (info.IsM3U)
                    {
                        var fileList = info.MakeFileListFromM3U();
                        foreach (FileInfo f in fileList)
                        {
                            mf.Add(new IndexedFileInfo(f));
                        }
                    }
                    else
                    {
                        string[] fileNames = System.IO.Directory.GetFiles(info.FileInfo.FullName);
                        IEnumerable<String> selectedList = from name in fileNames
                                                           where name.EndsWith(".mp3")
                                                           select name;

                        foreach (string n in selectedList)
                        {
                            mf.Add(new IndexedFileInfo(new FileInfo(n)));
                        }
                    }

                    MediaFiles = mf;
                    doubleSoundPlayer.Files = MediaFiles;
                }
            ));
        }

        public DelegateCommand PlayCommand { get; private set; }

        private IDialogService dialogService;

        private PlayerSetting playerSetting;

        private DelegateCommand showSettingDialogCommand;
        public DelegateCommand ShowSettingDialogCommand
        {
            get => showSettingDialogCommand ?? (showSettingDialogCommand = new DelegateCommand(
                () =>
                {
                    var param = new DialogParameters();
                    param.Add(nameof(PlayerSetting), playerSetting);
                    dialogService.ShowDialog(nameof(SettingWindow), param,
                        (IDialogResult result) =>
                        {
                            if (result.Parameters.GetValue<PlayerSetting>(nameof(SettingWindowViewModel.Setting)) == null)
                            {
                                // バツを押して閉じた時とかに値がNullになっている
                                return;
                            }
                            else
                            {
                                PlayerSetting pSettings = result.Parameters.GetValue<PlayerSetting>(nameof(SettingWindowViewModel.Setting));
                                doubleSoundPlayer.SwitchingDuration = pSettings.SwitchingDuration;
                                Properties.Settings.Default.SwitchinDuration = pSettings.SwitchingDuration;
                                Properties.Settings.Default.DefaultBaseDirectoryPath = pSettings.DefaultBaseDirectoryPath;
                                Properties.Settings.Default.FrontCut = pSettings.FrontCut;
                                Properties.Settings.Default.BackCut = pSettings.BackCut;
                                Properties.Settings.Default.Save();

                                DoubleSoundPlayer.FrontCut = pSettings.FrontCut;
                                DoubleSoundPlayer.BackCut = pSettings.BackCut;
                            }
                        }
                    );
                }
            ));
        }

        public DelegateCommand ShowLogWindowCommand
        {
            #region
            get => showLogWindowCommand ?? (showLogWindowCommand = new DelegateCommand(() =>
            {
                var param = new DialogParameters();
                dialogService.ShowDialog(nameof(HistoryWindow), param, (IDialogResult result) => { }
                );
            }));
        }
        private DelegateCommand showLogWindowCommand;
        #endregion


        public MainWindowViewModel(IDialogService _dialogService)
        {
            dialogService = _dialogService;

            string lastVisitedDirectoryPath = Properties.Settings.Default.lastVisitedDirectoryPath;
            var path = (new DirectoryInfo(Properties.Settings.Default.DefaultBaseDirectoryPath).Exists) ?
                Properties.Settings.Default.DefaultBaseDirectoryPath : @"C:\";

            doubleSoundPlayer = new DoubleSoundPlayer(
                new SoundPlayer(new WMPWrapper()),
                new SoundPlayer(new WMPWrapper())
            );

            TreeViewModel = new TreeViewModel(path);
            TreeViewModel.expandItemsTo(lastVisitedDirectoryPath);
            MediaFilesSettingCommand.Execute(TreeViewModel.SelectedItem);

            doubleSoundPlayer.CurrentDirectorySource = TreeViewModel;

            playerSetting = new PlayerSetting();
            playerSetting.DefaultBaseDirectoryPath = path;
            playerSetting.SwitchingDuration = Properties.Settings.Default.SwitchinDuration;
            playerSetting.BackCut = Properties.Settings.Default.BackCut;
            playerSetting.FrontCut = Properties.Settings.Default.FrontCut;

            DoubleSoundPlayer.SwitchingDuration = playerSetting.SwitchingDuration;
            DoubleSoundPlayer.Volume = Properties.Settings.Default.Volume;

            PlayCommand = new DelegateCommand(
                () =>
                {
                    doubleSoundPlayer.Files = MediaFiles;
                    doubleSoundPlayer.Play();
                },
                () => { return MediaFiles != null && MediaFiles.Count > 0; }
            ).ObservesProperty(() => MediaFiles);

        }

        private DelegateCommand randomSortCommand;
        public DelegateCommand RandomSortCommand
        {
            get => randomSortCommand ?? (randomSortCommand = new DelegateCommand(
                () =>
                {
                    Random r = new Random();
                    MediaFiles = MediaFiles.OrderBy(m => r.Next(MediaFiles.Count)).ToList();
                },
                () => MediaFiles.Count > 0
            )).ObservesProperty(() => MediaFiles);
        }

        private DelegateCommand nameOrderSortCommand;
        public DelegateCommand NameOrderSortCommand
        {
            get => nameOrderSortCommand ?? (nameOrderSortCommand = new DelegateCommand(
                () =>
                {
                    if (MediaFiles != null && MediaFiles.Count > 0)
                    {
                        MediaFiles = MediaFiles.OrderBy(m => m.Name).ToList();
                    }

                },
                () => MediaFiles.Count > 0
            )).ObservesProperty(() => MediaFiles);
        }

    }
}
