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
    public class DoubleSoundPlayer : BindableBase {
        private List<SoundPlayer> players;
        private PlayerIndex currentPlayerIndex = PlayerIndex.First;
        private Timer timer = new Timer(450);
        private bool mediaSwitching = false;
        private int switchingDuration = 0;
        private int volume = 100;

        private bool pausing = false;
        private bool autoStoped = false;

        enum PlayerIndex : int {
            First = 0,
            Second = 1

        }

        public DoubleSoundPlayer(SoundPlayer playerA, SoundPlayer playerB) {
            players = new List<SoundPlayer>(2); // 今の所、要素数２より大きくする必要はない
            SoundPlayer soundPlayerA = playerA;
            SoundPlayer soundPlayerB = playerB;

            soundPlayerA.mediaEndedEvent += nextPlay;
            soundPlayerB.mediaEndedEvent += nextPlay;

            players.Add(soundPlayerA);
            players.Add(soundPlayerB);

            timer.Elapsed += (source, e) => {
                timerEventHandler();
            };

            timer.Start();
        }

        private void timerEventHandler() {
            RaisePropertyChanged(nameof(PlayTime));
            SoundPlayer player = eitherBeforePlayEnd;
            if (player != null) {

                // ボリュームの操作等の曲の切り替え中の動作をここに記述する
                SoundPlayer otherPlayer = getOtherPlayer(player);

                if (autoStoped) {
                    // 何度もファイルの読み込みを行わないようにするため、一度自動停止したらここでリターンする。
                    // このフラグが立つタイミングは、ファイル再生の途中となるので、
                    // ファイル再生終了に対するイベントハンドラ内でフラグを初期化する
                    return;
                }

                if (!otherPlayer.Playing) {
                    if (Files.Count < PlayingIndex + 1) {
                        return;
                    }

                    PlayingIndex++;
                    otherPlayer.SoundFileInfo = Files[PlayingIndex];

                    // ここで行う曲の新規再生（切り替え）は BackCut, FrontCut を加味した play() メソッドで行う。
                    otherPlayer.play();
                    otherPlayer.Volume = 0;
                    otherPlayer.playStartedEvent += stopMedia;
                }

                int volumeUpAmount = (SwitchingDuration != 0) ? Volume / SwitchingDuration : 0;
                int volumeDownAmount = (SwitchingDuration != 0) ? Volume / SwitchingDuration : 0;

                if (otherPlayer.Volume + volumeUpAmount < Volume) {
                    otherPlayer.Volume += volumeUpAmount;
                }
                else {
                    otherPlayer.Volume = Volume;
                }

                player.Volume -= Volume / switchingDuration / 2;

            }
        }

        private void stopMedia(object sender) {
            SoundPlayer p = sender as SoundPlayer;
            if (p.Duration <= p.SecondsOfBeforeEndNotice * 2) {
                p.stop();
                PlayingIndex--;
                autoStoped = true;
            }
            else {
                // 再生継続の場合は曲名の更新通知を飛ばす
                mediaSwitching = true;
                RaisePropertyChanged(nameof(PlayingFileName));
            }

            p.playStartedEvent -= stopMedia;
        }

        /// <summary>
        /// テスト時に PrivateObject 経由でタイマーを止めるために使用
        /// </summary>
        private void stopTimer() {
            timer.Stop();
        }

        /// <summary>
        /// ２つのプレイヤーのうち、どちらか片方だけが終了直前のとき、そのプレイヤーを返します。
        /// 両方のプレイヤーが終了直前、または終了直前ではない場合、null を返します。
        /// </summary>
        private SoundPlayer eitherBeforePlayEnd {
            get {
                if (players[0].PassedBeforeEndPoint == players[1].PassedBeforeEndPoint) {
                    return null;
                }

                return players[0].PassedBeforeEndPoint ? players[0] : players[1];
            }
        }

        private void nextPlay(object sender) {
            autoStoped = false;
            SoundPlayer p = sender as SoundPlayer;
            var anotherPlayer = getOtherPlayer(p);

            // 逆側のプレイヤーが再生している状態の場合は、このハンドラ内で play() を呼び出すことは無い
            if (!anotherPlayer.Playing) {
                if (Files.Count > PlayingIndex + 1) {
                    PlayingIndex++;
                    p.SoundFileInfo = Files[PlayingIndex];
                    p.newPlay();
                }
            }

            mediaSwitching = false;
            RaisePropertyChanged(nameof(PlayingFileName));
        }

        public void play() {
            SelectedItem = Files[PlayingIndex];
            RaisePropertyChanged(nameof(PlayingFileName));
            CurrentPlayer.SoundFileInfo = Files[PlayingIndex];
            CurrentPlayer.newPlay();
        }

        public void updateSelectedFileName() {
            RaisePropertyChanged(nameof(PlayingFileName));
        }

        private DelegateCommand<object> playFromIndexCommand;
        public DelegateCommand<object> PlayFromIndexCommand {
            get => playFromIndexCommand ?? (playFromIndexCommand = new DelegateCommand<object>(
                (fi) => {
                    FileInfo f = (FileInfo)((ListViewItem)fi).Content;
                    SelectedItem = f;
                    CurrentPlayer.SoundFileInfo = f;
                    PlayingIndex = Files.IndexOf(f);
                    CurrentPlayer.newPlay();
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

        private DelegateCommand stopCommand;
        public DelegateCommand StopCommand {
            get => stopCommand ?? (stopCommand = new DelegateCommand(
                () => stop()
            ));
        }

        public DelegateCommand PauseOrResumeCommand{
            #region
            get => pauseOrResumeCommand ?? (pauseOrResumeCommand = new DelegateCommand(
                () => {
                    if (!pausing) {
                        pausing = true;
                        timer.Stop();
                        players[(int)PlayerIndex.First].pause();
                        players[(int)PlayerIndex.Second].pause();
                    }
                    else {
                        pausing = false;
                        timer.Start();
                        players[(int)PlayerIndex.First].resume();
                        players[(int)PlayerIndex.Second].resume();
                    }
                }
            ));
        }
        private DelegateCommand pauseOrResumeCommand;
        #endregion

        public String PlayTime {
            get {
                var p1 = players[(int)PlayerIndex.First];
                var p2 = players[(int)PlayerIndex.Second];

                TimeSpan currentPosition;
                TimeSpan currentDuration;
                if(p1.Playing == p2.Playing) {
                    currentPosition = new TimeSpan(0, 0, (int)Math.Max(p1.Position, p2.Position));
                    currentDuration = (p1.Position >= p2.Position) ? new TimeSpan(0, 0, (int)p1.Duration) : new TimeSpan(0, 0, (int)p2.Duration);
                }
                else {
                    currentPosition = (p1.Playing) ? new TimeSpan(0, 0, (int)p1.Position) : new TimeSpan(0, 0, (int)p2.Position);
                    currentDuration = (p1.Playing) ? new TimeSpan(0, 0, (int)p1.Duration) : new TimeSpan(0, 0, (int)p2.Duration);
                }

                return currentPosition.ToString(@"hh\:mm\:ss") + " / " + currentDuration.ToString(@"hh\:mm\:ss");
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
                else if (mediaSwitching && Files.Count > PlayingIndex) {
                    return Files[PlayingIndex -1].Name + " > " + Files[PlayingIndex].Name;
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

                Properties.Settings.Default.Volume = volume;
                Properties.Settings.Default.Save();
                RaisePropertyChanged(nameof(Volume));
            }
        }

        public ICurrentDirectorySource CurrentDirectorySource { private get; set; }

        public int FrontCut {
            get => players[(int)PlayerIndex.First].FrontCut;    // get は別に [0] でも [1] でも可。setは両方同時に行うので。
            set {
                players[(int)PlayerIndex.First].FrontCut = value;
                players[(int)PlayerIndex.Second].FrontCut = value;
            }
        }

        public int BackCut {
            get => players[(int)PlayerIndex.First].BackCut; 
            set {
                players[(int)PlayerIndex.First].BackCut = value;
                players[(int)PlayerIndex.Second].BackCut = value;
            }
        }
    }
}
