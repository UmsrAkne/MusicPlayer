using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Services.Dialogs;
using Prism.Commands;
using System.IO;

namespace MusicPlayer {
    class SettingWindowViewModel : BindableBase, IDialogAware {
        public string Title => "settingWindowTitle";

        public event Action<IDialogResult> RequestClose;

        private DialogParameters dialogParameters = new DialogParameters();

        private DelegateCommand yesCommand;
        public DelegateCommand YesCommand {
            get => yesCommand ?? (yesCommand = new DelegateCommand(
                () => {
                    // yesボタンを押したときの実行内容
                    //var ret = new DialogResult(ButtonResult.Yes, dialogParameters);
                    //dialogParameters.Add("key1", "str");
                    //this.RequestClose?.Invoke( ret );
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
