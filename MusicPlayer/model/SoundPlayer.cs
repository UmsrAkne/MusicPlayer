using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Navigation;
using WMPLib;

namespace MusicPlayer.model {

    delegate void MediaEndedEventHandler(object sender);

    class SoundPlayer {

        public SoundPlayer() {
            wmp.PlayStateChange += (int NewState) => {

                //  statusの番号については、MSのドキュメント "PlayStateChange Event of the AxWindowsMediaPlayer Object" を参照
                //  ここで使用する８番は再生終了時のステータスとなっている。
                if (NewState == 8) {
                    mediaEndedEvent(this);
                }
            };
        }

        private FileInfo soundFileInfo;
        public FileInfo SoundFileInfo {
            get { return soundFileInfo; }
            set {
                if (!value.Exists) throw new System.ArgumentException("指定されたファイルが存在しません " + value.FullName);
                soundFileInfo = value;
            }
        }

        private WindowsMediaPlayer wmp = new WindowsMediaPlayer();
        public event MediaEndedEventHandler mediaEndedEvent;

        public void play() {
            wmp.URL = soundFileInfo.FullName;
            Playing = true;
        }

        public void pause() {
            wmp.controls.pause();
            Playing = false;
        }

        public void resume() {
            wmp.controls.play();
            Playing = true;
        }

        public double Position {
            get {
                return wmp.controls.currentPosition;
            }
            set {
                wmp.controls.currentPosition = value;
            }
        }

        public double Duration {
            get {
                if (wmp.currentMedia != null) return wmp.currentMedia.duration;
                else return 0;
            }
        }

        public Boolean Playing {
            get;
            private set;
        } = false;
    }
}
