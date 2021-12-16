namespace MusicPlayer.Models
{
    using System.Collections.Generic;

    public interface IDatabase
    {
        List<History> List { get; }

        void Write(History history);
    }
}
