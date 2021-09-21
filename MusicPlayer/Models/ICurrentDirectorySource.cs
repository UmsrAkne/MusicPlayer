namespace MusicPlayer.Models
{
    using System.IO;

    public interface ICurrentDirectorySource
    {
        DirectoryInfo CurrentDirectoryInfo { get; }
    }
}
