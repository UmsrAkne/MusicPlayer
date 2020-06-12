using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MusicPlayer.model {
    class DoubleSoundPlayer {
        private List<SoundPlayer> players;
        private PlayerIndex currentPlayerIndex = PlayerIndex.First;

        enum PlayerIndex :int {
            First = 0,
            Second = 1
        }

        public DoubleSoundPlayer() {
            players = new List<SoundPlayer>(2); // 今の所、要素数２より大きくする必要はない
            players.Add(new SoundPlayer());
            players.Add(new SoundPlayer());
        }

        public void play() {
            CurrentPlayer.SoundFileInfo = Files[PlayingIndex];
            CurrentPlayer.play();
        }

        public List<FileInfo> Files {
            get; set;
        }

        public int PlayingIndex {
            get; set;
        } = 0;

        private void SwitchCurrentPlayerIndex() {
            if (currentPlayerIndex == PlayerIndex.First) currentPlayerIndex = PlayerIndex.Second;
            if (currentPlayerIndex == PlayerIndex.Second) currentPlayerIndex = PlayerIndex.First;
        }

        private SoundPlayer CurrentPlayer {
            get {
                return players[(int)currentPlayerIndex];
            }
        }
    }
}
