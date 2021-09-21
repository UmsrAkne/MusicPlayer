namespace MusicPlayer.Models
{
    using Prism.Mvvm;

    public class PlayerSetting : BindableBase
    {
        private int switchingDuration = 0;
        private string defaultBaseDirectoryPath;
        private int volume;
        private int frontCut;
        private int backCut;

        public int SwitchingDuration
        {
            get => switchingDuration;
            set
            {
                switchingDuration = value;
                RaisePropertyChanged();
            }
        }

        public string DefaultBaseDirectoryPath
        {
            get => defaultBaseDirectoryPath;
            set
            {
                defaultBaseDirectoryPath = value;
                RaisePropertyChanged();
            }
        }

        public int Volume
        {
            get => volume;
            set
            {
                volume = value;
                RaisePropertyChanged();
            }
        }

        public int FrontCut
        {
            get => frontCut;
            set => SetProperty(ref frontCut, value);
        }

        public int BackCut
        {
            get => backCut;
            set => SetProperty(ref backCut, value);
        }
    }
}
