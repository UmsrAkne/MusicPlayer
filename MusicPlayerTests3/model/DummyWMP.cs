using MusicPlayer.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayerTests3.model {
    class DummyWMP : IPlayer {

        private bool playing = false;
        public bool Playing => playing;

        private bool loading = false;
        public bool Loading => loading;

        private string url;
        public string URL {
            get => url;
            set {
                url = value;

                // 実際のWMPの挙動として、
                // 何故か URL をセットした時点で再生が開始されるため(?)、このようなコードがここに入る。
                playing = true;
                loading = true;
            }
        }

        public int Volume { get; set; } = 100;
        public double Position { get; set; }

        public double NextMediaDuration { get; set; }
        private double duration = 0;
        public double Duration => duration;

        public event EventHandler mediaEnded;
        public event EventHandler mediaStarted;

        public void pause() {
            throw new NotImplementedException();
        }

        public void play() {
            playing = true;
            loading = true;
        }

        public void stop() {
            playing = false;
        }

        public void forward() {
            if (loading) {
                loading = false;
                duration = NextMediaDuration;
                mediaStarted(this, new EventArgs());
            }

            if (!playing) {
                return;
            }

            Position += 0.3;

            if(Duration < Position) {
                mediaEnded(this, new EventArgs());
                Position = Duration;
                playing = false;
            }
        }
    }
}
