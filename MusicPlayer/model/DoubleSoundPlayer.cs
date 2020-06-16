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
        private int switchingDuration = 0;

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

        /// <summary>
        /// playersに入っているplayerのうち、引数に指定された方ではない側のプレイヤーを取得します。
        /// </summary>
        /// <param name="soundPlayer"></param>
        private SoundPlayer getOtherPlayer(SoundPlayer soundPlayer) {
           if(soundPlayer != players[(int)PlayerIndex.First] && soundPlayer != players[(int)PlayerIndex.Second]) {
                throw new ArgumentException("入力された SoundPlayer は、playersに格納されている SoundPlayer ではありません");
            }

            if (players[(int)PlayerIndex.First] != soundPlayer) return players[(int)PlayerIndex.First];
            else return players[(int)PlayerIndex.Second];
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

        /// <summary>
        /// ２つのメディアファイルのフェードイン・アウト切り替えに要する時間を秒で指定します。
        /// </summary>
        public int SwitchingDuration {
            get {
                return switchingDuration;
            }
            set {
                switchingDuration = value;
                players[(int)PlayerIndex.First].SecondsOfBeforeEndNotice = value;
                players[(int)PlayerIndex.Second].SecondsOfBeforeEndNotice = value;
            }
        }
    }
}
