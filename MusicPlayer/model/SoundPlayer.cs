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

    public delegate void MediaEndedEventHandler(object sender);
    public delegate void PlayStartedEventHandler(object sender);

    public class SoundPlayer {

        public SoundPlayer(IPlayer player) {

            this.player = player;
            this.player.Volume = 100;

            this.player.mediaEnded += (sender,e) => {
                mediaEndedEvent?.Invoke(this);
            };

            this.player.mediaStarted += (sender, e) => {
                Duration = this.player.Duration;
                playStartedEvent?.Invoke(this);
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

        private IPlayer player;
        public event MediaEndedEventHandler mediaEndedEvent;
        public event PlayStartedEventHandler playStartedEvent;

        /// <summary>
        /// 新規で再生を開始する際に使用します。
        /// </summary>
        public void newPlay() {
            player.URL = soundFileInfo.FullName;
            player.play();

            /** ここで情報を記録するので、再生中のメディアが終了N秒前に入ってこのメソッドが呼び出されると、
             *  最後に再生していたメディアが更新される。
             *  最後の方まで再生した曲は、視聴を終えたとみなす。
             */
            Properties.Settings.Default.lastPlayingFileName = SoundFileInfo.FullName;
            Properties.Settings.Default.Save();

            var logFileName = "playlog.txt";
            var text = (File.Exists(logFileName)) ? File.ReadAllText(logFileName) : "";

            using (StreamWriter sw = new StreamWriter(logFileName, false)) {
                sw.WriteLine($"{DateTime.Now.ToString("yyy/MM/dd HH:mm:ss")},{SoundFileInfo.FullName}");
                sw.Write(text);
            }
        }

        /// <summary>
        /// FrontCut の値だけ、冒頭をカットして再生を開始します。
        /// </summary>
        public void play() {
            newPlay();

            if (BeforeEndPointPassageNotification) {
                // クロスフェードによる音量の変更を伴う場合にのみ FrontCut を有効にする。
                player.Position = FrontCut;
            }
        }

        public void pause() {
            player.pause();
        }

        public void resume() {
            player.resume();
        }

        public void stop() {
            player.stop();
        }

        public int Volume {
            get {
                return player.Volume;
            }
            set {
                if(value > 100) {
                    player.Volume = 100;
                }
                else if(value < 0) {
                    player.Volume = 0;
                }
                else {
                    player.Volume = value;
                }
            }
        }

        public double Position {
            get {
                return player.Position;
            }
            set {
                player.Position = value;
            }
        }

        public bool Loading => player.Loading;

        public double Duration { get; private set; } = 0;

        public Boolean Playing {
            get => player.Playing;
            private set{
            }
        }

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
                if (Duration == 0 
                    || SecondsOfBeforeEndNotice * 2 > Duration 
                    || !Playing
                    || !BeforeEndPointPassageNotification
                    ) {
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

        /// <summary>
        /// falseに設定すると、PassedBeforeEndPoint が常に false を返すようになり、
        /// 結果的に クロスフェード機能を無効にします。
        /// </summary>
        public bool BeforeEndPointPassageNotification {
            get; set;
        } = true;

        /// <summary>
        /// SoundFileInfo に対して、空の FileInfo を直接セットします。
        /// </summary>
        public void setEmptyFileInfo() {
            soundFileInfo = new FileInfo("notExistEmptyFileInfo");
        }
    }
}
