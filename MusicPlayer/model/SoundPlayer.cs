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
using System.Timers;
using System.Windows.Media;
using System.Windows.Navigation;
using WMPLib;

namespace MusicPlayer.model {

    delegate void MediaEndedEventHandler(object sender);
    delegate void MediaBeforeEndEventHandler(object sender);

    class SoundPlayer {

        public SoundPlayer() {
            wmp.PlayStateChange += (int NewState) => {

                //  statusの番号については、MSのドキュメント "PlayStateChange Event of the AxWindowsMediaPlayer Object" を参照
                //  ここで使用する８番は再生終了時のステータスとなっている。
                if (NewState == 8) {
                    mediaEndedEvent(this);
                }
            };

            timer.Elapsed += (sender, e) => {
                if(Duration > 0 && Position >= Duration - SecondsOfBeforeEndNotice) {
                    if (!hasNotifiedBeforeEnd) {
                        mediaBeforeEndEvent(this);
                        hasNotifiedBeforeEnd = true;
                    }
                }
            };

            timer.Start();
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
        private Timer timer = new Timer(1000);
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

        /// <summary>
        /// SoundPlayerオブジェクトは現在流れている曲の終了N秒前になるとイベントを送出する。
        /// このとき、上記の N の値はこのプロパティにセットした値となる。
        /// </summary>
        public int SecondsOfBeforeEndNotice {
            get; set;
        }
    }
}
