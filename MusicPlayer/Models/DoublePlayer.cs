namespace MusicPlayer.Models
{
    using System;
    using System.Collections.ObjectModel;
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

        public DoublePlayer(ISoundProvider soundProvider)
        {
            SoundProvider = soundProvider;
            Sounds = new ObservableCollection<ISound>();
            Sounds.CollectionChanged += (e, sender) => RaisePropertyChanged(nameof(Sounds));

            playTimeTimer.Elapsed += (e, sender) => { TimerEventHandler(); };
            playTimeTimer.Start();

            timer.Elapsed += (e, sender) => Fader();
            timer.Start();
        }

        public int Volume { get => volume; set => SetProperty(ref volume, value); }

        public int PlayingIndex { get => playingIndex; set => SetProperty(ref playingIndex, value); }

        public int SwitchingDuration { get => switchingDuration; set => SetProperty(ref switchingDuration, value); }

        public bool Switching { get; private set; }

        public ISoundProvider SoundProvider { get; private set; }

        public ObservableCollection<ISound> Sounds { get; }

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
                        nextSound.Volume = 0;
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

            int timerExecuteCountPerSec = 4;
            int amount = Volume / Math.Max(SwitchingDuration - 2, 1) / timerExecuteCountPerSec;
            Sounds.First().Volume -= amount;
            Sounds.Last().Volume += (int)(amount * 1.5);
        }
    }
}
