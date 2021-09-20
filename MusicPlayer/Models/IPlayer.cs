namespace MusicPlayer.Models
{
    using System;

    public interface IPlayer
    {
        event EventHandler mediaEnded;
        event EventHandler mediaStarted;

        void play();
        void stop();
        void pause();
        void resume();
        bool Playing { get; }
        bool Loading { get; }
        string URL { get; set; }
        int Volume { get; set; }
        double Position { get; set; }
        double Duration { get; }
    }
}
