namespace MusicPlayer.Models
{
    using System.Collections.Generic;
    using System.Linq;

    public class VolumeController
    {
        public int ExecuteCountPerSec { get; set; } = 4;

        public int SwitchingDuration { get; set; } = 15;

        public float MaxVolume { get; set; } = 1.0f;

        private List<ISound> PlayingSounds { get; set; } = new List<ISound>();

        public void AddPlayingSound(ISound sound)
        {
            PlayingSounds.Add(sound);
        }

        public void Fader()
        {
            PlayingSounds = PlayingSounds.Where(sound => sound.Playing).ToList();

            float changeAmount = MaxVolume / SwitchingDuration / ExecuteCountPerSec * 1.02f;

            PlayingSounds.Where(sound => sound.Duration > SwitchingDuration * 2.5).ToList().ForEach(sound =>
            {
                // 曲の開始部分の音量調節処理
                if (sound.Position < SwitchingDuration && sound.Volume <= MaxVolume)
                {
                    sound.Volume += changeAmount;
                    if (sound.Volume >= MaxVolume)
                    {
                        sound.Volume = MaxVolume;
                    }
                }

                // 曲の終了部分の音量調節処理
                if (sound.Position > sound.Duration - SwitchingDuration && sound.Volume >= 0)
                {
                    sound.Volume -= changeAmount;
                    if (sound.Volume <= 0)
                    {
                        sound.Volume = 0;
                    }
                }
            });
        }
    }
}