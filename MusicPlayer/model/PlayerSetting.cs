﻿using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        
    }
}
