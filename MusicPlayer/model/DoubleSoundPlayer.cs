using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.model {
    class DoubleSoundPlayer {
        private List<SoundPlayer> players;

        public DoubleSoundPlayer() {
            players = new List<SoundPlayer>(2); // 今の所、要素数２より大きくする必要はない
            players.Add(new SoundPlayer());
            players.Add(new SoundPlayer());
        }
    }
}
