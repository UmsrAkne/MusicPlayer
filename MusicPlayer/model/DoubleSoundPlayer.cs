using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            SoundPlayer soundPlayerA = new SoundPlayer();
            soundPlayerA.SecondsOfBeforeEndNotice = 5;
            SoundPlayer soundPlayerB = new SoundPlayer();
            soundPlayerB.SecondsOfBeforeEndNotice = 5;

            players.Add(soundPlayerA);
            players.Add(soundPlayerB);

            soundPlayerA.mediaBeforeEndEvent += DoubleSoundPlayer_mediaBeforeEndEvent;
            soundPlayerB.mediaBeforeEndEvent += DoubleSoundPlayer_mediaBeforeEndEvent;

            soundPlayerA.mediaEndedEvent += DoubleSoundPlayer_mediaEndedEvent;
            soundPlayerB.mediaEndedEvent += DoubleSoundPlayer_mediaEndedEvent;
        }

        private void DoubleSoundPlayer_mediaEndedEvent(object sender) {
            PlayingIndex += 1;
        }

        private void DoubleSoundPlayer_mediaBeforeEndEvent(object sender) {
            // イベントを送出したプレイヤーでない方のプレイヤーに対して操作を行う
            // なので、センダーではない方のプレイヤーを代入する
            SoundPlayer player = null;
            if((SoundPlayer)sender != players[(int)PlayerIndex.First]) {
                player = players[(int)PlayerIndex.First];
            }
            else {
                player = players[(int)PlayerIndex.Second];
            }

            if(Files.Count > PlayingIndex + 1) {
                player.SoundFileInfo = Files[PlayingIndex + 1];
                player.play();
            }
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

        private void SwitchPlayer() {
            players.Reverse();
        }

        private SoundPlayer CurrentPlayer {
            get {
                return players[(int)currentPlayerIndex];
            }
        }

        /// <summary>
        /// 現在カレントプレイヤーではない側のプレイヤーを取得します
        /// </summary>
        private SoundPlayer SubPlayer {
            get {
                if (CurrentPlayer != players[(int)PlayerIndex.First]) return players[(int)PlayerIndex.First];
                else return players[(int)PlayerIndex.Second];
            }
        }
    }
}
