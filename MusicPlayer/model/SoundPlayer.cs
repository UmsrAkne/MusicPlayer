using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WMPLib;

namespace MusicPlayer.model {
    class SoundPlayer {

        private FileInfo soundFileInfo;
        public FileInfo SoundFileInfo {
            get { return soundFileInfo; }
            set {
                if (!value.Exists) throw new System.ArgumentException("指定されたファイルが存在しません " + value.FullName);
                soundFileInfo = value;
            }
        }

        private WindowsMediaPlayer wmp = new WindowsMediaPlayer();

        public void play() {
            wmp.URL = soundFileInfo.FullName;
        }

        public void pause() {
            wmp.controls.pause();
        }

        public void resume() {
            wmp.controls.play();
        }

        public double Position {
            get {
                return wmp.controls.currentPosition;
            }
            set {
                wmp.controls.currentPosition = value;
            }
        }
    }
}
