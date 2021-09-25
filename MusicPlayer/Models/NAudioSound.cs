namespace MusicPlayer.Models
{
    using System;

    public class NAudioSound : ISound
    {
        private string url;

        public event EventHandler MediaEnded;

        public event EventHandler MediaStarted;

        public event EventHandler LoadCompleted;

        public event EventHandler NearTheEnd;

        public bool Playing => throw new NotImplementedException();

        public bool Loading => throw new NotImplementedException();

        public string URL { get => url; set => url = value; }

        public int Volume { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public double Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public double Duration => throw new NotImplementedException();

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Play()
        {
            throw new NotImplementedException();
        }

        public void Resume()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
