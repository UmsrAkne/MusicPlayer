namespace MusicPlayerTests3.Models
{
    using System.Collections.Generic;
    using MusicPlayer.Models;

    public class DummyDB : IDatabase
    {
        public List<History> List => throw new System.NotImplementedException();

        public void Write(History history)
        {
        }
    }
}
