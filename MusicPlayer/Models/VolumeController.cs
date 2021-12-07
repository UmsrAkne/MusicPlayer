namespace MusicPlayer.Models
{
    using System.Collections.Generic;

    public class VolumeController
    {
        public int ExecuteCountPerSec { get; set; }

        private List<ISound> PlayingSounds { get; } = new List<ISound>();

        public void AddPlayingSound(ISound sound)
        {
            PlayingSounds.Add(sound);
        }

        public void Fader()
        {
        }
    }
}
