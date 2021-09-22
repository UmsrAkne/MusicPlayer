namespace MusicPlayer.Models
{
    using System.Collections.Generic;
    using System.Timers;
    using Prism.Mvvm;

    public class DoublePlayer : BindableBase
    {
        private int volume = 100;
        private Timer timer = new Timer(200);

        public DoublePlayer(ISound soundA, ISound soundB)
        {
            Sounds = new List<ISound>() { soundA, soundB };
            Sounds.Capacity = 2;
            timer.Elapsed += (e, sender) => Fader();
        }

        public int Volume { get => volume; set => SetProperty(ref volume, value); }

        private List<ISound> Sounds { get; }

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
