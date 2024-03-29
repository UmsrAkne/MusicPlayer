﻿namespace MusicPlayer.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MusicPlayer.Models;
    using MusicPlayer.Views;
    using Prism.Commands;
    using Prism.Mvvm;
    using Prism.Services.Dialogs;

    public class MainWindowViewModel : BindableBase
    {
        private List<IndexedFileInfo> mediaFiles = new List<IndexedFileInfo>();
        private DelegateCommand<object> mediaFilesSettingCommand;
        private IDialogService dialogService;
        private PlayerSetting playerSetting;
        private DelegateCommand stopCommand;
        private DelegateCommand showSettingDialogCommand;
        private DelegateCommand showLogWindowCommand;
        private DelegateCommand nameOrderSortCommand;
        private DelegateCommand randomSortCommand;
        private DelegateCommand playNextCommand;
        private DelegateCommand playBackCommand;

        private ISound selectedItem;

        public MainWindowViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;

            string lastVisitedDirectoryPath = Properties.Settings.Default.lastVisitedDirectoryPath;
            var path = new DirectoryInfo(Properties.Settings.Default.DefaultBaseDirectoryPath).Exists ?
                Properties.Settings.Default.DefaultBaseDirectoryPath : @"C:\";

            var db = new HistoryDbContext();
            db.Database.EnsureCreated();
            SoundProvider = new SoundProvider(db);

            DoublePlayer = new DoublePlayer(SoundProvider);

            TreeViewModel = new TreeViewModel(path);
            TreeViewModel.ExpandItemsTo(lastVisitedDirectoryPath);
            MediaFilesSettingCommand.Execute(TreeViewModel.SelectedItem);

            playerSetting = new PlayerSetting();
            playerSetting.DefaultBaseDirectoryPath = path;
            playerSetting.SwitchingDuration = Properties.Settings.Default.SwitchinDuration;
            playerSetting.BackCut = Properties.Settings.Default.BackCut;
            playerSetting.FrontCut = Properties.Settings.Default.FrontCut;

            DoublePlayer.Volume = Properties.Settings.Default.Volume;
            DoublePlayer.FrontCut = playerSetting.FrontCut;
            DoublePlayer.BackCut = playerSetting.BackCut;

            DoublePlayer.SwitchingDuration = playerSetting.SwitchingDuration;

            PlayCommand = new DelegateCommand(
                () =>
                {
                    SoundProvider.Sounds.Clear();
                    SoundProvider.ViewingSounds.ForEach(s => SoundProvider.Sounds.Add(s));
                    DoublePlayer.Play();
                });
        }

        public DoublePlayer DoublePlayer { get; private set; }

        public SoundProvider SoundProvider { get; private set; }

        public TreeViewModel TreeViewModel
        {
            get;
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

        public DelegateCommand<object> MediaFilesSettingCommand
        {
            get => mediaFilesSettingCommand ?? (mediaFilesSettingCommand = new DelegateCommand<object>(
                (object param) =>
                {
                    MediaDirectory info = (MediaDirectory)param;
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
                        IEnumerable<string> selectedList = from name in fileNames
                                                           where name.EndsWith(".mp3")
                                                           select name;

                        foreach (string n in selectedList)
                        {
                            mf.Add(new IndexedFileInfo(new FileInfo(n)));
                        }
                    }

                    var histories = SoundProvider.GetListenHistory(info.Name);

                    var snds =
                        Enumerable.Range(0, mf.Count)
                        .Select(cnt => (ISound)new NAudioSound() { Index = cnt + 1, URL = mf[cnt].FullName }).ToList();

                    snds.ForEach(s =>
                    {
                        var existingHistory = histories.Where(h => h.FullName == s.URL).FirstOrDefault();
                        s.ListenCount = existingHistory != null ? existingHistory.ListenCount : 0;
                    });

                    SoundProvider.ViewingSounds = snds;

                    Task _ = LoadSounds(SoundProvider.ViewingSounds);
                }));
        }

        public DelegateCommand PlayCommand { get; private set; }

        public DelegateCommand StopCommand
        {
            get => stopCommand ?? (stopCommand = new DelegateCommand(() =>
            {
                DoublePlayer.Stop();
            }));
        }

        public DelegateCommand ShowSettingDialogCommand
        {
            get => showSettingDialogCommand ?? (showSettingDialogCommand = new DelegateCommand(() =>
            {
                var param = new DialogParameters();
                param.Add(nameof(PlayerSetting), playerSetting);

                dialogService.ShowDialog(
                    nameof(SettingWindow),
                    param,
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
                        Properties.Settings.Default.SwitchinDuration = pSettings.SwitchingDuration;
                        Properties.Settings.Default.DefaultBaseDirectoryPath = pSettings.DefaultBaseDirectoryPath;
                        Properties.Settings.Default.FrontCut = pSettings.FrontCut;
                        Properties.Settings.Default.BackCut = pSettings.BackCut;
                        Properties.Settings.Default.Save();

                        DoublePlayer.FrontCut = pSettings.FrontCut;
                        DoublePlayer.BackCut = pSettings.BackCut;
                    }
                });
            }));
        }

        public ISound SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem != null)
                {
                    selectedItem.IsSelected = false;
                }

                if (value != null)
                {
                    value.IsSelected = true;
                }

                SetProperty(ref selectedItem, value);
            }
        }

        public WindowRect WindowRect { get; private set; } = new WindowRect();

        public DelegateCommand ShowLogWindowCommand
        {
            get => showLogWindowCommand ?? (showLogWindowCommand = new DelegateCommand(() =>
            {
                var param = new DialogParameters();
                dialogService.ShowDialog(nameof(HistoryWindow), param, (IDialogResult result) => { });
            }));
        }

        public DelegateCommand RandomSortCommand
        {
            get => randomSortCommand ?? (randomSortCommand = new DelegateCommand(() =>
            {
                List<ISound> randomOrderList = SoundProvider.ViewingSounds.OrderBy(i => Guid.NewGuid()).ToList();
                Enumerable.Range(0, randomOrderList.Count).ToList().ForEach(i => (randomOrderList[i] as NAudioSound).Index = i + 1);
                SoundProvider.ViewingSounds = randomOrderList;
            }));
        }

        public DelegateCommand NameOrderSortCommand
        {
            get => nameOrderSortCommand ?? (nameOrderSortCommand = new DelegateCommand(() =>
            {
                List<ISound> nameOrderList = SoundProvider.ViewingSounds = SoundProvider.ViewingSounds.OrderBy(s => s.Name).ToList();
                Enumerable.Range(0, nameOrderList.Count).ToList().ForEach(i => (nameOrderList[i] as NAudioSound).Index = i + 1);
                SoundProvider.ViewingSounds = nameOrderList;
            }));
        }

        public DelegateCommand PlayNextCommand
        {
            get => playNextCommand ?? (playNextCommand = new DelegateCommand(() => DoublePlayer.Next()));
        }

        public DelegateCommand PlayBackCommand
        {
            get => playBackCommand ?? (playBackCommand = new DelegateCommand(() => DoublePlayer.Back()));
        }

        private async Task LoadSounds(List<ISound> sounds) => await Task.Run(() => sounds.ForEach(s => s.Load()));
    }
}
