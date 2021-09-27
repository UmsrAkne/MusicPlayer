namespace MusicPlayer.Models
{
    using System;
    using NAudio.Wave;

    public class NAudioSound : ISound
    {
        private int volume;
        private WaveOut waveOut;
        private Mp3FileReader reader;

        public event EventHandler MediaEnded;

        public event EventHandler MediaStarted;

        public event EventHandler LoadCompleted;

        public event EventHandler NearTheEnd;

        public bool Playing { get; private set; }

        public bool Loading => throw new NotImplementedException();

        public string URL { get; set; }

        public int Volume { get => volume; set => volume = value; }

        public double Position { get => reader != null ? reader.CurrentTime.TotalMilliseconds : 0; set => throw new NotImplementedException(); }

        public double Duration { get; private set; } = 0;

        public void Load()
        {
            if (!string.IsNullOrEmpty(URL))
            {
                using (reader = new Mp3FileReader(URL))
                {
                    reader = new Mp3FileReader(URL);
                    Duration = reader.TotalTime.TotalMilliseconds;
                }
            }
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Play()
        {
            if (!string.IsNullOrEmpty(URL))
            {
                reader = new Mp3FileReader(URL);
                reader.Position = 0;

                Duration = reader.TotalTime.TotalMilliseconds;
                waveOut = new WaveOut();
                waveOut.Init(reader);
                waveOut.Play();
                Playing = true;

                waveOut.PlaybackStopped += (sender, e) =>
                {
                    Playing = false;
                    MediaEnded?.Invoke(this, EventArgs.Empty);

                    reader.Dispose();
                    reader = null;
                    waveOut.Dispose();
                    waveOut = null;
                };
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
