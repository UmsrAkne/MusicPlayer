﻿namespace MusicPlayer.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MusicPlayer.Models;
    using Prism.Commands;
    using Prism.Services.Dialogs;

    public class HistoryWindowViewModel : IDialogAware
    {
        private HistoryDbContext historyDBContext = new HistoryDbContext();

        public HistoryWindowViewModel()
        {
            CloseDialogCommand = new DelegateCommand(() => RequestClose?.Invoke(new DialogResult()));

            // 履歴の取得は、最後に聴いた曲から新しい順に 200 件を取得する。
            Histories = historyDBContext.Histories.OrderByDescending(h => h.LastListenDate).Take(200).Select(h => h).ToList();
        }

        public event Action<IDialogResult> RequestClose;

        public string Title => "log";

        public List<History> Histories { get; set; }

        public DelegateCommand CloseDialogCommand { get; private set; }

        public bool CanCloseDialog() => true;

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }
    }
}
