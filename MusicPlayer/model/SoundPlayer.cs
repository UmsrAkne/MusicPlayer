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
                    mediaEndedEvent?.Invoke(this);
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
            wmp.controls.currentPosition = FrontCut;
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
        /// PassedBeforeEndPoint 実行時、メディアが終了直前と判定されるポイントを指定します。
        /// </summary>
        public int SecondsOfBeforeEndNotice {
            get; set;
        }

        /// <summary>
        /// このメディアが終了直前のポイントを通り過ぎたかどうかを取得します。
        /// 終了直前のポイントとは、"Duration - SecondsOfBeforeEndNotice" の値が示す時点です。
        /// </summary>
        public bool PassedBeforeEndPoint {
            get {
                if (Duration == 0) {
                    return false;
                }
                return (Position >= Duration - SecondsOfBeforeEndNotice - BackCut);
            }
        }

        /// <summary>
        /// このメディアの実際の Duration から BackCut を引いた時点を通過したかどうかを取得します。
        /// このプロパティが true を返す場合、実質的にメディアの再生が終了していることを意味します。
        /// </summary>
        public bool PssedBackCutPoint {
            get => (Position >= Duration - BackCut);
        }

        /// <summary>
        /// 曲の先頭部分を指定秒数カットします。
        /// </summary>
        public int FrontCut {
            get; set;
        }

        /// <summary>
        /// 曲の終端部分を指定秒数カットします。
        /// </summary>
        public int BackCut {
            get; set;
        }
    }
}
