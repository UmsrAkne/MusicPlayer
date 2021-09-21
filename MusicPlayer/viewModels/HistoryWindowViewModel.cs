namespace MusicPlayer.viewModels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Prism.Commands;
    using Prism.Services.Dialogs;

    public class HistoryWindowViewModel : IDialogAware
    {
        public HistoryWindowViewModel()
        {
            CloseDialogCommand = new DelegateCommand(() => RequestClose?.Invoke(new DialogResult()));

            var logFileName = "playlog.txt";
            Log = (File.Exists(logFileName)) ? File.ReadAllText(logFileName) : "履歴は存在しません";
        }

        public string Title => "log";

        public string Log { get; private set; }

        public DelegateCommand CloseDialogCommand { get; private set; }

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog() => true;

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }
    }
}
