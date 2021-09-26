﻿namespace MusicPlayer.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class SoundProvider : ISoundProvider
    {
        public List<ISound> Sounds { get; private set; } = new List<ISound>();

        public ISound GetSound(int index)
        {
            return Sounds[index];
        }
    }
}
