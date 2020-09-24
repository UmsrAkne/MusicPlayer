using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MusicPlayer.model {
    public class PlayerSetting : BindableBase {

        private int switchingDuration = 0;
        public int SwitchingDuration {
            get => switchingDuration;
            set {
                switchingDuration = value;
                RaisePropertyChanged();
            }
        }

        private string defaultBaseDirectoryPath;
        public string DefaultBaseDirectoryPath {
            get => defaultBaseDirectoryPath;
            set {
                defaultBaseDirectoryPath = value;
                RaisePropertyChanged();
            }
        }

        private int volume;
        public int Volume {
            get => volume;
            set {
                volume = value;
                RaisePropertyChanged();
            }
        }

        private int frontCut;
        public int FrontCut {
            get => frontCut;
            set => SetProperty(ref frontCut, value);
        }

        private int backCut;
        public int BackCut {
            get => backCut;
            set => SetProperty(ref backCut, value);
        }


    }
}
