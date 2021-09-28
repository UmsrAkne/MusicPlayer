namespace MusicPlayer.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Timers;
    using Prism.Mvvm;

    public class DoublePlayer : BindableBase
    {
        private int volume = 100;
        private Timer timer = new Timer(250);
        private Timer playTimeTimer = new Timer(1000);
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

        public DoublePlayer(ISoundProvider soundProvider)
        {
            SoundProvider = soundProvider;
            Sounds = new List<ISound>();

            playTimeTimer.Elapsed += (e, sender) => { TimerEventHandler(); };
        }

        public int Volume { get => volume; set => SetProperty(ref volume, value); }

        public List<FileInfo> PlayList { get; } = new List<FileInfo>();

        public int PlayingIndex { get => playingIndex; set => SetProperty(ref playingIndex, value); }

        public int SwitchingDuration { get; set; }

        public bool Switching { get; private set; }

        public ISoundProvider SoundProvider { get; private set; }

        private List<ISound> Sounds { get; }

        public void TimerEventHandler()
        {
            if (Sounds.Count == 1 && SoundProvider.Count > PlayingIndex + 1)
            {
                ISound currentSound = Sounds[0];
                bool isLongSound = currentSound.Duration >= SwitchingDuration * 1000 * 2.5;
                bool soundIsEnding = currentSound.Position >= currentSound.Duration - (SwitchingDuration * 1000);

                if (isLongSound && soundIsEnding)
                {
                    var nextSound = SoundProvider.GetSound(++PlayingIndex);
                    Sounds.Add(nextSound);
                    nextSound.MediaEnded += NextSound;

                    if (nextSound.Duration >= SwitchingDuration * 1000 * 2.5)
                    {
                        nextSound.Play();
                        Switching = true;
                    }
                }
            }
        }

        public void Play()
        {
            PlayingIndex = 0;
            Sounds.Clear();
            ISound sound = SoundProvider.GetSound(PlayingIndex);
            Sounds.Add(sound);
            sound.Play();
            playTimeTimer.Start();

            sound.MediaEnded += NextSound;
        }

        public void NextSound(object sender, EventArgs e)
        {
            Switching = false;
            ISound snd = sender as ISound;
            Sounds.RemoveAt(Sounds.IndexOf(snd));
            snd.MediaEnded -= NextSound;

            if (SoundProvider.Count <= PlayingIndex + 1)
            {
                // 次に再生できる曲がなければ終了
                return;
            }

            if (Sounds.Count == 0)
            {
                ISound nextSound = SoundProvider.GetSound(++PlayingIndex);
                Sounds.Add(nextSound);
                nextSound.Play();
                nextSound.MediaEnded += NextSound;
            }
            else if (!Sounds.Last().Playing)
            {
                Sounds.Last().Play();
            }
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
            if (PlayingIndex + 1 < PlayList.Count)
            {
                PlayingIndex++;
                var nextSound = GetOtherSound((ISound)sender);
                nextSound.LoadCompleted += LoadCompletedEventHandler;
                nextSound.URL = PlayList[PlayingIndex].FullName;
            }
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
