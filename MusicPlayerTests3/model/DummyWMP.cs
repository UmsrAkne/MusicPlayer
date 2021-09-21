namespace MusicPlayerTests3.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MusicPlayer.Models;

    public class DummyWMP : IPlayer
    {
        private bool playing = false;
        private bool loading = false;
        private double duration = 0;
        private string url;

        public event EventHandler MediaEnded;

        public event EventHandler MediaStarted;

        public bool Loading => loading;

        public bool Playing => playing;

        public string URL
        {
            get => url;
            set
            {
                url = value;

                // 実際のWMPの挙動として、
                // 何故か URL をセットした時点で再生が開始されるため(?)、このようなコードがここに入る。
                playing = true;
                loading = true;
                Position = 0;
            }
        }

        public int Volume { get; set; } = 100;

        public double Position { get; set; }

        public double NextMediaDuration { get; set; }

        public double Duration => duration;

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Play()
        {
            playing = true;
            loading = true;
        }

        public void Stop()
        {
            playing = false;
        }

        public void Resume()
        {
            throw new NotImplementedException();
        }

        public void Forward()
        {
            if (loading)
            {
                loading = false;
                duration = NextMediaDuration;
                MediaStarted(this, new EventArgs());
            }

            if (!playing)
            {
                return;
            }

            Position += 0.2;

            if (Duration < Position)
            {
                Position = Duration;
                playing = false;
                MediaEnded(this, new EventArgs());
            }
        }
    }
}
