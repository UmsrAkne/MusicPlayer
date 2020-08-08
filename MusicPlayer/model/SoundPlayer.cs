using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    delegate void MediaBeforeEndEventHandler(object sender);
    delegate void PlayStartedEventHandler(object sender);

    class SoundPlayer {

        public SoundPlayer() {
            wmp.settings.volume = 100;

            wmp.PlayStateChange += (int NewState) => {
                if (NewState == (int)WMPPlayState.wmppsPlaying) {
                    Duration = wmp.currentMedia.duration;
                    if (Duration < SecondsOfBeforeEndNotice * 2) hasNotifiedBeforeEnd = true;
                    else hasNotifiedBeforeEnd = false;
                    playStartedEvent?.Invoke(this);
                }
            };

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
        public event MediaBeforeEndEventHandler mediaBeforeEndEvent;
        public event PlayStartedEventHandler playStartedEvent;
        private Boolean hasNotifiedBeforeEnd = false;

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

        public void stop() {
            wmp.controls.stop();
            Playing = false;
        }

        public int Volume {
            get {
                return wmp.settings.volume;
            }
            set {
                if (value > 100 || value < 0) throw new ArgumentException("ボリュームの値は 0 - 100 の値を入力してください");
                wmp.settings.volume = value;
            }
        }

        public double Position {
            get {
                return wmp.controls.currentPosition;
            }
            set {
                if(value >= Duration - SecondsOfBeforeEndNotice) {
                    hasNotifiedBeforeEnd = false;
                }

                wmp.controls.currentPosition = value;
            }
        }

        public double Duration { get; private set; } = 0;

        public Boolean Playing {
            get;
            private set;
        } = false;

        /// <summary>
        /// SoundPlayerオブジェクトは現在流れている曲の終了N秒前になるとイベントを送出する。
        /// このとき、上記の N の値はこのプロパティにセットした値となる。
        /// </summary>
        public int SecondsOfBeforeEndNotice {
            get; set;
        }
    }
}
