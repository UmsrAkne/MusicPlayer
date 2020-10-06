using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMPLib;

namespace MusicPlayer.model {
    public class WMPWrapper : IPlayer {

        private WindowsMediaPlayer wmp;

        public WMPWrapper() {
            wmp = new WindowsMediaPlayer();
            wmp.PlayStateChange += wmpPlayStateChangeEventHandler;
        }

        public bool Playing => wmp.playState == WMPPlayState.wmppsPlaying;

        /// <summary>
        /// URLをセットした時点で true になり、
        /// wmp のステータスが WMPPlayState.wmppsPlaying に変化した時点で false になります。
        /// </summary>
        public bool Loading {
            get;
            private set;
        } 

        public string URL {
            get => wmp.URL;
            set {
                wmp.URL = value;
                Loading = true;
            }
        }

        public int Volume {
            get => wmp.settings.volume;
            set => wmp.settings.volume = value;
        }

        public double Position {
            get => wmp.controls.currentPosition;
            set => wmp.controls.currentPosition = value;
        }

        public double Duration { get => wmp.currentMedia.duration; }

        public event EventHandler mediaEnded;
        public event EventHandler mediaStarted;

        public void pause() {
            wmp.controls.pause();
        }

        public void resume() {
            wmp.controls.play();
        }

        public void play() {
            wmp.PlayStateChange -= wmpPlayStateChangeEventHandler;
            string url = URL; // 一時退避
            wmp.close();

            wmp = new WindowsMediaPlayer();
            wmp.PlayStateChange += wmpPlayStateChangeEventHandler;
            URL = url;
            wmp.settings.volume = Volume;
            wmp.controls.play();
        }

        public void stop() {
            wmp.controls.stop();
        }

        private void wmpPlayStateChangeEventHandler(int NewState) {
            if(NewState == (int)WMPPlayState.wmppsPlaying) {
                mediaStarted?.Invoke(this, new EventArgs());
                Loading = false;
            }

            if(NewState == (int)WMPPlayState.wmppsMediaEnded) {
                mediaEnded?.Invoke(this, new EventArgs());
            }
        }
    }
}
