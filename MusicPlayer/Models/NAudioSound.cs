﻿namespace MusicPlayer.Models
{
    using System;
    using System.IO;
    using NAudio.Wave;

    public class NAudioSound : ISound
    {
        private int volume = 100;
        private WaveOutEvent waveOut;
        private AudioFileReader reader;

        public event EventHandler MediaEnded;

        public event EventHandler MediaStarted;

        public event EventHandler LoadCompleted;

        public event EventHandler NearTheEnd;

        public bool Playing { get; private set; }

        public bool Loading => throw new NotImplementedException();

        public string URL { get; set; }

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

        public double Duration { get; private set; } = 0;

        public void Load()
        {
            System.Diagnostics.Debug.WriteLine($"execute NAudio.Load() {URL}");
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
            System.Diagnostics.Debug.WriteLine($"execute NAudio.Play() {URL}");
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
            throw new NotImplementedException();
        }
    }
}
