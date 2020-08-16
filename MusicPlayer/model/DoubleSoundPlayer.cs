using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;

namespace MusicPlayer.model {
    class DoubleSoundPlayer : BindableBase{
        private List<SoundPlayer> players;
        private PlayerIndex currentPlayerIndex = PlayerIndex.First;
        private Timer timer = new Timer(450);
        private bool mediaSwitching = false;
        private int switchingDuration = 0;
        private int volume = 100;

        enum PlayerIndex :int {
            First = 0,
            Second = 1

        }

        public DoubleSoundPlayer() {
            players = new List<SoundPlayer>(2); // 今の所、要素数２より大きくする必要はない
            SoundPlayer soundPlayerA = new SoundPlayer();
            SoundPlayer soundPlayerB = new SoundPlayer();

            players.Add(soundPlayerA);
            players.Add(soundPlayerB);

            timer.Elapsed += (source, e) => {
                RaisePropertyChanged(nameof(PlayTime));
                SoundPlayer player = eitherBeforePlayEnd;
                if(player != null) {
                    SoundPlayer otherPlayer = getOtherPlayer(player);

                    if (!otherPlayer.Playing) {
                        PlayingIndex++;
                        otherPlayer.SoundFileInfo = Files[PlayingIndex];
                        otherPlayer.play();
                    }

                }
            };

            timer.Start();
        }

        /// <summary>
        /// ２つのプレイヤーのうち、どちらか片方だけが終了直前のとき、そのプレイヤーを返します。
        /// 両方のプレイヤーが終了直前、または終了直前ではない場合、null を返します。
        /// </summary>
        private SoundPlayer eitherBeforePlayEnd {
            get {
                if(players[0].PassedBeforeEndPoint == players[1].PassedBeforeEndPoint){
                    return null;
                }

                return players[0].PassedBeforeEndPoint ? players[0] : players[1];
            }
        }

        public void play() {
            SelectedItem = Files[PlayingIndex];
            RaisePropertyChanged(nameof(PlayingFileName));
            CurrentPlayer.SoundFileInfo = Files[PlayingIndex];
            CurrentPlayer.play();
        }

        private DelegateCommand<object> playFromIndexCommand;
        public DelegateCommand<object> PlayFromIndexCommand {
            get => playFromIndexCommand ?? (playFromIndexCommand = new DelegateCommand<object>(
                (fi) => {
                    FileInfo f = (FileInfo)((ListViewItem)fi).Content;
                    SelectedItem = f;
                    CurrentPlayer.SoundFileInfo = f;
                    PlayingIndex = Files.IndexOf(f);
                    CurrentPlayer.play();
                    RaisePropertyChanged(nameof(PlayingFileName));
                }
            ));
        }

        public void stop() {
            PlayingIndex = 0;
            CurrentPlayer.stop();
            getOtherPlayer(CurrentPlayer).stop();

            CurrentPlayer.Volume = this.Volume;
            getOtherPlayer(CurrentPlayer).Volume = this.Volume;

            CurrentPlayer.SoundFileInfo = Files[PlayingIndex];
            mediaSwitching = false;

        }

        public String PlayTime {
            get {
                var player = players[(int)PlayerIndex.First];
                var currentElapsedTime = new TimeSpan(0, 0, (int)player.Position);
                var currentDuration = new TimeSpan(0, 0, (int)player.Duration);
                return currentElapsedTime.ToString(@"hh\:mm\:ss") + " / " + currentDuration.ToString(@"hh\:mm\:ss");
            }
        }

        public List<FileInfo> Files {
            get; set;
        }

        public int PlayingIndex {
            get; set;
        } = 0;

        private FileInfo selectedItem;
        public FileInfo SelectedItem {
            get => selectedItem;
            set {
                selectedItem = value;
                RaisePropertyChanged();
            }
        }

        public String PlayingFileName {
            get {
                if (Files == null || Files.Count <= PlayingIndex) return "";
                else if (mediaSwitching && Files.Count > PlayingIndex + 1) {
                    return Files[PlayingIndex].Name + " > " + Files[PlayingIndex + 1].Name;
                }
                return Files[PlayingIndex].Name;
            }
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

        public Boolean Playing {
            get {
                return players[(int)PlayerIndex.First].Playing || players[(int)PlayerIndex.Second].Playing;
            }
        }

        public int Volume {
            get { return volume; }
            set {
                volume = value;
                if (value >= 100) volume = 100;
                if (value <= 0) volume = 0;

                players[(int)PlayerIndex.First].Volume = volume;
                players[(int)PlayerIndex.Second].Volume = volume;
            }
        }
    }
}
