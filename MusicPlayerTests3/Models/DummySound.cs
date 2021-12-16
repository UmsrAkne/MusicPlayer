namespace MusicPlayerTests3.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MusicPlayer.Models;

    internal class DummySound : ISound
    {
        private string url;
        private bool noticedNearTheEnd;
        private float volume = 100;

        public event EventHandler MediaEnded;

        public event EventHandler MediaStarted;

        public event EventHandler LoadCompleted;

        public event EventHandler NearTheEnd;

        public bool Playing { get; private set; }

        public bool Loading => throw new NotImplementedException();

        public string URL
        {
            get => url;
            set
            {
                url = value;
                Position = 0;
                noticedNearTheEnd = false;
                LoadCompleted?.Invoke(this, EventArgs.Empty);
            }
        }

        public float Volume
        {
            get => volume;
            set => volume = Math.Max(Math.Min(1, value), 0);
        }

        public double Position { get; set; }

        public double Duration { get; set; }

        public int SwitchingDuration { get; set; }

        public bool IsSelected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Name => throw new NotImplementedException();

        public int FrontCut { get; set; }

        public int BackCut { get; set; }

        public int ListenCount { get; set; }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Play()
        {
            if (Duration > 0)
            {
                Playing = true;
            }
        }

        public void Resume()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            Playing = false;
            Position = 0;
        }

        public void Forward(double time)
        {
            if (!Playing)
            {
                return;
            }

            Position += time;

            if (Position >= Duration)
            {
                Playing = false;
                MediaEnded?.Invoke(this, EventArgs.Empty);
            }
            else if (SwitchingDuration > 0 && !noticedNearTheEnd && Position >= Duration - SwitchingDuration)
            {
                NearTheEnd?.Invoke(this, EventArgs.Empty);
                noticedNearTheEnd = true;
            }
        }

        public void Load()
        {
        }
    }
}
