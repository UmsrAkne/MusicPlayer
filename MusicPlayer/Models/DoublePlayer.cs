namespace MusicPlayer.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Timers;
    using Prism.Mvvm;

    public class DoublePlayer : BindableBase
    {
        private int volume = 100;
        private Timer timer = new Timer(250);
        private int playingIndex;
        private int switchingDuration = 0;

        public DoublePlayer(ISound soundA, ISound soundB)
        {
            Sounds = new List<ISound>() { soundA, soundB };
            Sounds.Capacity = 2;

            void playNext(object sender, EventArgs e)
            {
                if (PlayingIndex + 1 < PlayList.Count && !GetOtherSound((ISound)sender).Playing)
                {
                    PlayingIndex++;
                    ISound sound = (ISound)sender;
                    sound.Stop();
                    sound.URL = PlayList[PlayingIndex].FullName;
                    sound.Play();
                }
            }

            Sounds[0].MediaEnded += playNext;
            Sounds[1].MediaEnded += playNext;

            Sounds[0].NearTheEnd += LoadSound;
            Sounds[1].NearTheEnd += LoadSound;

            timer.Elapsed += (e, sender) => Fader();
            timer.Start();
        }

        public int Volume { get => volume; set => SetProperty(ref volume, value); }

        public List<FileInfo> PlayList { get; } = new List<FileInfo>();

        public int PlayingIndex { get => playingIndex; set => SetProperty(ref playingIndex, value); }

        public int SwitchingDuration { get; set; }

        public bool Switching { get; private set; }

        private List<ISound> Sounds { get; }

        public void Play()
        {
            PlayingIndex = 0;
            Sounds.ForEach(sound =>
            {
                sound.Stop();
                sound.Volume = Volume;
            });

            ISound s = Sounds[0];
            s.URL = PlayList[PlayingIndex].FullName;
            s.Play();
        }

        public void Play(string url)
        {
            Sounds.ForEach(sound =>
            {
                sound.Stop();
                sound.Volume = Volume;
            });

            ISound s = Sounds[0];
            s.URL = url;
            s.Play();
        }

        /// <summary>
        /// 必要なタイミング（クロスフェード時等）に音量を調整するメソッドです。
        /// Timer から定期的に呼び出します。
        /// </summary>
        public void Fader()
        {
            if (SwitchingDuration == 0 || !Switching)
            {
                return;
            }

            ISound endingSound = null;

            foreach (ISound sound in Sounds)
            {
                if (sound.Position >= sound.Duration - SwitchingDuration)
                {
                    endingSound = sound;
                    break;
                }
            }

            if (endingSound != null)
            {
                int timerExecuteCountPerSec = 4;
                int amount = Volume / SwitchingDuration / timerExecuteCountPerSec;
                endingSound.Volume -= amount;
                GetOtherSound(endingSound).Volume += amount;

                if (endingSound.Volume <= 0)
                {
                    Switching = false;
                }
            }
        }

        private void LoadSound(object sender, EventArgs e)
        {
            PlayingIndex++;
            var nextSound = GetOtherSound((ISound)sender);
            nextSound.LoadCompleted += LoadCompletedEventHandler;
            nextSound.URL = PlayList[PlayingIndex].FullName;
        }

        private void LoadCompletedEventHandler(object sender, EventArgs e)
        {
            var sound = (ISound)sender;
            if (sound.Duration > SwitchingDuration * 2.5)
            {
                sound.Volume = 0;
                sound.Play();
                Switching = true;
            }
            else
            {
                PlayingIndex--;
            }

            sound.LoadCompleted -= LoadCompletedEventHandler;
        }

        private ISound GetOtherSound(ISound sound) => object.ReferenceEquals(sound, Sounds[0]) ? Sounds[1] : Sounds[0];
    }
}
