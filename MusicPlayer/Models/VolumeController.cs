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

            if (SwitchingDuration == 0 || ExecuteCountPerSec == 0)
            {
                return;
            }

            float changeAmount = MaxVolume / SwitchingDuration / ExecuteCountPerSec * 1.02f;

            int switchingDurationMilliSec = SwitchingDuration * 1000;

            PlayingSounds.Where(sound => sound.Duration > switchingDurationMilliSec * 2.5).ToList().ForEach(sound =>
            {
                // 曲の開始部分の音量調節処理
                if (sound.Position < switchingDurationMilliSec && sound.Volume <= MaxVolume)
                {
                    sound.Volume += changeAmount;
                    if (sound.Volume >= MaxVolume)
                    {
                        sound.Volume = MaxVolume;
                    }
                }

                // 曲の終了部分の音量調節処理
                if (sound.Position > sound.Duration - switchingDurationMilliSec && sound.Volume >= 0)
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