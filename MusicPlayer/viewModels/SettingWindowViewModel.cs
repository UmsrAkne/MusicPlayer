namespace MusicPlayer
{
    using System;
    using MusicPlayer.Models;
    using Prism.Commands;
    using Prism.Mvvm;
    using Prism.Services.Dialogs;

    public class SettingWindowViewModel : BindableBase, IDialogAware
    {
        private DialogParameters dialogParameters = new DialogParameters();
        private PlayerSetting setting = new PlayerSetting();
        private DelegateCommand settingFinishCommand;

        public SettingWindowViewModel()
        {
            CloseDialogCommand = new DelegateCommand(() => RequestClose?.Invoke(new DialogResult()));
        }

        public event Action<IDialogResult> RequestClose;

        public string Title => "settingWindowTitle";

        public PlayerSetting Setting
        {
            get => setting;
            set
            {
                setting = value;
                RaisePropertyChanged();
            }
        }

        public DelegateCommand SettingFinishCommand
        {
            get => settingFinishCommand ?? (settingFinishCommand = new DelegateCommand(() =>
            {
                var ret = new DialogResult(ButtonResult.Yes, dialogParameters);
                dialogParameters.Add(nameof(Setting), Setting);
                this.RequestClose?.Invoke(ret);
            }));
        }

        public DelegateCommand CloseDialogCommand { get; set; }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Setting = parameters.GetValue<PlayerSetting>(nameof(PlayerSetting));
        }

        public virtual bool CanCloseDialog() => true;

        public virtual void OnDialogClosed()
        {
        }
    }
}
