using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.model {
    class MediaDirectory {

        public String Name { get; set; }
        public List<MediaDirectory> ChildDirectory { get; set; }

    }
}
