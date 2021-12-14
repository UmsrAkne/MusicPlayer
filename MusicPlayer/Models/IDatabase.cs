namespace MusicPlayer.Models
{
    public interface IDatabase
    {
        void Write(History history);
    }
}
