namespace MusicPlayer.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Prism.Mvvm;

    public class SoundProvider : BindableBase, ISoundProvider
    {
        private List<ISound> viewingSounds = new List<ISound>();

        public SoundProvider()
        {
            DbContext = new HistoryDbContext();
            DbContext.Database.EnsureCreated();
        }

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

        private HistoryDbContext DbContext { get; }

        public ISound GetSound(int index)
        {
            ISound s = Sounds[index];
            s.ListenCount++;
            s.Load();

            History history = new History();
            history.FullName = s.URL;
            history.LastListenDate = DateTime.Now;
            history.DirectoryName = new DirectoryInfo(Path.GetDirectoryName(s.URL)).Name;
            history.ListenCount = s.ListenCount;
            DbContext.Write(history);

            return s;
        }

        public List<History> GetListenHistory(string directoryName)
        {
            return DbContext.Histories.Where(h => h.DirectoryName == directoryName).ToList();
        }
    }
}
