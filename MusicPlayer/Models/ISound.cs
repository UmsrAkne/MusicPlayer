namespace MusicPlayer.Models
{
    using System;

    public interface ISound
    {
        event EventHandler MediaEnded;

        bool Playing { get; }

        bool Loading { get; }

        bool IsSelected { get; set; }

        string URL { get; set; }

        string Name { get; }

        float Volume { get; set; }

        double Position { get; set; }

        double Duration { get; }

        int FrontCut { get; set; }

        int BackCut { get; set; }

        int ListenCount { get; set; }

        void Load();

        void Play();

        void Stop();

        void Pause();

        void Resume();
    }
}
