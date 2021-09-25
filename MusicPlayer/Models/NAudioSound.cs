namespace MusicPlayer.Models
{
    using System;
    using NAudio.Wave;

    public class NAudioSound : ISound
    {
        private int volume;
        private WaveOut waveOut;

        public event EventHandler MediaEnded;

        public event EventHandler MediaStarted;

        public event EventHandler LoadCompleted;

        public event EventHandler NearTheEnd;

        public bool Playing => throw new NotImplementedException();

        public bool Loading => throw new NotImplementedException();

        public string URL { get; set; }

        public int Volume { get => volume; set => volume = value; }

        public double Position { get => waveOut.GetPosition(); set => throw new NotImplementedException(); }

        public double Duration { get; private set; }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Play()
        {
            if (!string.IsNullOrEmpty(URL))
            {
                Mp3FileReader reader = new Mp3FileReader(URL);
                Duration = reader.TotalTime.TotalMilliseconds;
                waveOut = new WaveOut();
                waveOut.Init(reader);
                waveOut.Play();
            }
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
