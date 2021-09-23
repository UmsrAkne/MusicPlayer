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
        private Timer timer = new Timer(200);
        private int playingIndex;
        private int switchingDuration = 0;

        public DoublePlayer(ISound soundA, ISound soundB)
        {
            Sounds = new List<ISound>() { soundA, soundB };
            Sounds.Capacity = 2;

            void playNext(object sender, EventArgs e)
            {
                PlayingIndex++;
                ISound sound = (ISound)sender;
                sound.Stop();
                sound.URL = PlayList[PlayingIndex].FullName;
                sound.Play();
            }

            Sounds[0].MediaEnded += playNext;
            Sounds[1].MediaEnded += playNext;

            timer.Elapsed += (e, sender) => Fader();
        }

        public int Volume { get => volume; set => SetProperty(ref volume, value); }

        public List<FileInfo> PlayList { get; } = new List<FileInfo>();

        public int PlayingIndex { get => playingIndex; set => SetProperty(ref playingIndex, value); }

        public int SwitchingDuration { get; set; }

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
        }
    }
}
