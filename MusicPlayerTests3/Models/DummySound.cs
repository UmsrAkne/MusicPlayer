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
        public event EventHandler MediaEnded;

        public event EventHandler MediaStarted;

        public bool Playing { get; private set; }

        public bool Loading => throw new NotImplementedException();

        public string URL { get; set; }

        public int Volume { get; set; }

        public double Position { get; set; }

        public double Duration { get; set; }

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
            Position += time;
            if (Position >= Duration)
            {
                Playing = false;
                MediaEnded?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
