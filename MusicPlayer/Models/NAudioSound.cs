namespace MusicPlayer.Models
{
    using System;
    using System.IO;
    using NAudio.Wave;
    using Prism.Mvvm;

    public class NAudioSound : BindableBase, ISound
    {
        private int volume = 100;
        private WaveOutEvent waveOut;
        private AudioFileReader reader;
        private bool isSelected;
        private bool playing;
        private double duration = 0;

        public event EventHandler MediaEnded;

        public bool Playing { get => playing; private set => SetProperty(ref playing, value); }

        public bool Loading => throw new NotImplementedException();

        public string URL { get; set; }

        public string Name => Path.GetFileName(URL);

        public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value); }

        public int Index { get; set; }

        public int Volume
        {
            get => volume;
            set
            {
                volume = Math.Max(Math.Min(value, 100), 0);
                reader.Volume = (float)((float)volume / 100.0);
            }
        }

        public double Position { get => reader != null ? reader.CurrentTime.TotalMilliseconds : 0; set => throw new NotImplementedException(); }

        public double Duration { get => duration; private set => SetProperty(ref duration, value); }

        public int FrontCut { get; set; }

        public int BackCut { get; set; }

        public void Load()
        {
            if (!string.IsNullOrEmpty(URL))
            {
                using (var r = new Mp3FileReader(URL))
                {
                    Duration = r.TotalTime.TotalMilliseconds;
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
                reader = new AudioFileReader(URL);
                reader.Position = 0;

                Duration = reader.TotalTime.TotalMilliseconds;
                waveOut = new WaveOutEvent();
                waveOut.Init(reader);
                waveOut.Volume = (float)((float)Volume / 100.0);
                waveOut.Play();
                Playing = true;

                waveOut.PlaybackStopped += (sender, e) =>
                {
                    Playing = false;
                    MediaEnded?.Invoke(this, EventArgs.Empty);
                    reader.Dispose();
                    waveOut.Dispose();
                };
            }
        }

        public void Resume()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            waveOut.Stop();
            Playing = false;
        }
    }
}
