using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.Models
{
    public interface ICurrentDirectorySource
    {
        DirectoryInfo CurrentDirectoryInfo { get; }
    }
}
