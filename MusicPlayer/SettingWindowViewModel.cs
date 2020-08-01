using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Services.Dialogs;
using Prism.Commands;
using System.IO;
using MusicPlayer.model;

namespace MusicPlayer {
    class SettingWindowViewModel : BindableBase, IDialogAware {
        public string Title => "settingWindowTitle";

        public event Action<IDialogResult> RequestClose;

        private DialogParameters dialogParameters = new DialogParameters();
        private PlayerSetting setting = new PlayerSetting();
        public PlayerSetting Setting {
            get => setting;
        }

        private DelegateCommand settingFinishCommand;
        public DelegateCommand SettingFinishCommand {
            get => settingFinishCommand ?? (settingFinishCommand = new DelegateCommand(
                () => {
                    var ret = new DialogResult(ButtonResult.Yes, dialogParameters);
                    dialogParameters.Add(nameof(Setting), setting);
                    this.RequestClose?.Invoke( ret );
                }
            ));
        }

        public DelegateCommand CloseDialogCommand { get; set; }

        public SettingWindowViewModel() {
            CloseDialogCommand = new DelegateCommand(() => RequestClose?.Invoke(new DialogResult()));

        }

        public void OnDialogOpened(IDialogParameters parameters) {
        }

        public virtual bool CanCloseDialog() {
            return true;
        }

        public virtual void OnDialogClosed() {

        }

    }
}
