namespace MusicPlayer.Models
{
    using System;

    public interface IPlayer
    {
        event EventHandler MediaEnded;

        event EventHandler MediaStarted;

        bool Playing { get; }

        bool Loading { get; }

        string URL { get; set; }

        int Volume { get; set; }

        double Position { get; set; }

        double Duration { get; }

        void Play();

        void Stop();

        void Pause();

        void Resume();
    }
}
