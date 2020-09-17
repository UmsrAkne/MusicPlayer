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
        }

        public bool Playing => wmp.playState == WMPPlayState.wmppsPlaying;

        public bool Loading => throw new NotImplementedException();

        public string URL {
            get => wmp.URL;
            set => wmp.URL = value;
        }

        public int Volume {
            get => wmp.settings.volume;
            set => wmp.settings.volume = value;
        }

        public double Position {
            get => wmp.controls.currentPosition;
            set => wmp.controls.currentPosition = value;
        }

        public double Duration { get; }

        public event EventHandler mediaEnded;
        public event EventHandler mediaStarted;

        public void pause() {
            throw new NotImplementedException();
        }

        public void play() {
            throw new NotImplementedException();
        }

        public void stop() {
            throw new NotImplementedException();
        }
    }
}
