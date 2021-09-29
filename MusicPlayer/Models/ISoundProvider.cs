namespace MusicPlayer.Models
{
    public interface ISoundProvider
    {
        int Count { get; }

        ISound GetSound(int index);
    }
}
