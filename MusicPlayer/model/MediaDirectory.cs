using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.model {
    class MediaDirectory {

        public String Name {
            get {
                if (FileInfo == null) return "";
                else return FileInfo.Name;
            }
        }

        public List<MediaDirectory> ChildDirectory { get; set; }
        public FileInfo FileInfo { get; set; }

    }
}
