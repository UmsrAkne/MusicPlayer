namespace MusicPlayer.Models
{
    using Prism.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class SoundProvider : BindableBase, ISoundProvider
    {
        private List<ISound> viewingSounds = new List<ISound>();

        public List<ISound> Sounds { get; private set; } = new List<ISound>();

        public List<ISound> ViewingSounds
        {
            get => viewingSounds;
            set
            {
                if (Sounds.Count != 0 && value.Count != 0 && Path.GetDirectoryName(Sounds[0].URL) == Path.GetDirectoryName(value[0].URL))
                {
                    value = Sounds;
                }

                SetProperty(ref viewingSounds, value);
            }
        }

        public int Count => Sounds.Count;

        public ISound GetSound(int index)
        {
            ISound s = Sounds[index];
            s.Load();
            return s;
        }
    }
}
