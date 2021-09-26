namespace MusicPlayer.Models
{
    public interface ISoundProvider
    {
        ISound GetSound(int index);
    }
}
