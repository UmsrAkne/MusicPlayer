﻿using System;
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
    public delegate void MediaBeforeEndEventHandler(object sender);
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
                if (Duration < SecondsOfBeforeEndNotice * 2) hasNotifiedBeforeEnd = true;
                else hasNotifiedBeforeEnd = false;
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
        private Boolean hasNotifiedBeforeEnd = false;

        public void play() {
            player.URL = soundFileInfo.FullName;
            player.play();
            player.Position = FrontCut;
        }

        public void pause() {
        }

        public void resume() {
            player.play();
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
                if(value >= Duration - SecondsOfBeforeEndNotice) {
                    hasNotifiedBeforeEnd = false;
                }
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
                if (Duration == 0 || SecondsOfBeforeEndNotice * 2 > Duration || !Playing) {
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
