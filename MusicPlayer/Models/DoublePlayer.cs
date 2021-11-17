﻿namespace MusicPlayer.Models
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
        private double playTime;
        private double duration;

        public DoublePlayer(ISoundProvider soundProvider)
        {
            SoundProvider = soundProvider;
            Sounds = new ObservableCollection<ISound>();
            Sounds.CollectionChanged += (e, sender) =>
            {
                RaisePropertyChanged(nameof(Sounds));
                ObservableCollection<ISound> list = e as ObservableCollection<ISound>;
                if (list.Count > 0)
                {
                    Duration = list.First().Duration;
                }
            };

            playTimeTimer.Elapsed += (e, sender) => { TimerEventHandler(); };
            playTimeTimer.Start();

            timer.Elapsed += (e, sender) => Fader();
        }

        public int Volume
        {
            get => volume;
            set
            {
                Sounds.ToList().ForEach(s => s.Volume = value);
                SetProperty(ref volume, value);
            }
        }

        public int PlayingIndex { get => playingIndex; set => SetProperty(ref playingIndex, value); }

        public int SwitchingDuration { get => switchingDuration; set => SetProperty(ref switchingDuration, value); }

        public bool Switching { get; private set; }

        public double PlayTime { get => playTime; set => SetProperty(ref playTime, value); }

        public double Duration { get => duration; set => SetProperty(ref duration, value); }

        public int FrontCut { get; set; }

        public int BackCut { get; set; }

        public ISoundProvider SoundProvider { get; private set; }

        public ObservableCollection<ISound> Sounds { get; }

        public void TimerEventHandler()
        {
            if (Sounds.Count > 0)
            {
                // 現在の再生位置の更新処理
                PlayTime = Sounds.First().Position;
            }

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

                    if (nextSound.Duration >= (SwitchingDuration * 1000 * 2.5) + (BackCut * 1000))
                    {
                        nextSound.FrontCut = FrontCut;
                        nextSound.Play();
                        nextSound.Volume = 0;
                        Switching = true;
                    }
                }
            }
        }

        public void Play()
        {
            //// 最初から再生するので、既に曲を再生中の場合も考慮して、一度 Stop() する
            Stop();

            //// 以降が新規再生の処理

            ISound sound = SoundProvider.GetSound(PlayingIndex);
            Sounds.Add(sound);
            sound.Play();
            sound.MediaEnded += NextSound;

            playTimeTimer.Start();
            timer.Start();
        }

        public void Stop()
        {
            Switching = false;
            playTimeTimer.Stop();
            timer.Stop();

            PlayingIndex = 0;
            Sounds.ToList().ForEach(s =>
            {
                s.MediaEnded -= NextSound;
                s.Stop();
            });

            Sounds.Clear();
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
            decimal amount = Math.Ceiling((decimal)Volume / Math.Max(SwitchingDuration - 2, 1) / timerExecuteCountPerSec);
            Sounds.First().Volume -= (int)amount;
            Sounds.Last().Volume += (int)(amount * 1.5m);
        }
    }
}
