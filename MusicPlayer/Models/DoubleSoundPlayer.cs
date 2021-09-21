namespace MusicPlayer.Models
{
    using System;
    using System.Collections.Generic;
    using System.Timers;
    using System.Windows.Controls;
    using Prism.Commands;
    using Prism.Mvvm;

    public class DoubleSoundPlayer : BindableBase
    {
        private List<SoundPlayer> players;
        private Timer timer = new Timer(450);
        private bool mediaSwitching = false;
        private int switchingDuration = 0;
        private int volume = 100;
        private bool crossFade = true;
        private IndexedFileInfo selectedItem;
        private int selectedIndex = 0;

        private DelegateCommand toNextCommand;
        private DelegateCommand toBackCommand;
        private DelegateCommand<object> playFromIndexCommand;
        private DelegateCommand stopCommand;
        private DelegateCommand pauseOrResumeCommand;

        private bool pausing = false;
        private bool autoStoped = false;

        public DoubleSoundPlayer(SoundPlayer playerA, SoundPlayer playerB)
        {
            players = new List<SoundPlayer>(2); // 今の所、要素数２より大きくする必要はない
            SoundPlayer soundPlayerA = playerA;
            SoundPlayer soundPlayerB = playerB;

            soundPlayerA.MediaEndedEvent += NextPlay;
            soundPlayerB.MediaEndedEvent += NextPlay;

            players.Add(soundPlayerA);
            players.Add(soundPlayerB);

            timer.Elapsed += (source, e) => TimerEventHandler();

            timer.Start();
        }

        private enum PlayerIndex : int
        {
            First = 0,
            Second = 1
        }

        /// <summary>
        /// ２つのメディアファイルのフェードイン・アウト切り替えに要する時間を秒で指定します。
        /// </summary>
        public int SwitchingDuration
        {
            get => switchingDuration;
            set
            {
                switchingDuration = value;
                players[(int)PlayerIndex.First].SecondsOfBeforeEndNotice = value;
                players[(int)PlayerIndex.Second].SecondsOfBeforeEndNotice = value;
            }
        }

        public bool Playing
        {
            get => players[(int)PlayerIndex.First].Playing || players[(int)PlayerIndex.Second].Playing;
        }

        public int Volume
        {
            get => volume;
            set
            {
                volume = value;

                if (value >= 100)
                {
                    volume = 100;
                }

                if (value <= 0)
                {
                    volume = 0;
                }

                players[(int)PlayerIndex.First].Volume = volume;
                players[(int)PlayerIndex.Second].Volume = volume;

                Properties.Settings.Default.Volume = volume;
                Properties.Settings.Default.Save();
                RaisePropertyChanged(nameof(Volume));
            }
        }

        public ICurrentDirectorySource CurrentDirectorySource { private get; set; }

        public int FrontCut
        {
            get => players[(int)PlayerIndex.First].FrontCut;    // get は別に [0] でも [1] でも可。setは両方同時に行うので。
            set
            {
                players[(int)PlayerIndex.First].FrontCut = value;
                players[(int)PlayerIndex.Second].FrontCut = value;
            }
        }

        public int BackCut
        {
            get => players[(int)PlayerIndex.First].BackCut;
            set
            {
                players[(int)PlayerIndex.First].BackCut = value;
                players[(int)PlayerIndex.Second].BackCut = value;
            }
        }

        public bool CrossFade
        {
            #region
            get => crossFade;
            set
            {
                players[(int)PlayerIndex.First].BeforeEndPointPassageNotification = value;
                players[(int)PlayerIndex.Second].BeforeEndPointPassageNotification = value;
                SetProperty(ref crossFade, value);
            }
        }
        #endregion

        public DelegateCommand ToNextCommand => toNextCommand ?? (toNextCommand = new DelegateCommand(() =>
        {
            if (PlayingIndex < Files.Count - 1)
            {
                Play(PlayingIndex + 1);
            }
        }));

        public DelegateCommand ToBackCommand => toBackCommand ?? (toBackCommand = new DelegateCommand(() =>
        {
            if (PlayingIndex > 0 && Files.Count != 0)
            {
                Play(PlayingIndex - 1);
            }
        }));

        public DelegateCommand<object> PlayFromIndexCommand => playFromIndexCommand ?? (playFromIndexCommand = new DelegateCommand<object>((fi) =>
        {
            IndexedFileInfo f = (IndexedFileInfo)((ListViewItem)fi).Content;
            SelectedItem = f;
            CurrentPlayer.SoundFileInfo = f.FileInfo;
            PlayingIndex = Files.IndexOf(f);
            CurrentPlayer.NewPlayer();
            RaisePropertyChanged(nameof(PlayingFileName));
        }));

        public DelegateCommand StopCommand => stopCommand ?? (stopCommand = new DelegateCommand(() => Stop()));

        public DelegateCommand PauseOrResumeCommand => pauseOrResumeCommand ?? (pauseOrResumeCommand = new DelegateCommand(() =>
        {
            if (!pausing)
            {
                pausing = true;
                timer.Stop();
                players[(int)PlayerIndex.First].Pause();
                players[(int)PlayerIndex.Second].Pause();
            }
            else
            {
                pausing = false;
                timer.Start();
                players[(int)PlayerIndex.First].Resume();
                players[(int)PlayerIndex.Second].Resume();
            }
        }));

        public string PlayTime
        {
            get
            {
                var p1 = players[(int)PlayerIndex.First];
                var p2 = players[(int)PlayerIndex.Second];

                TimeSpan currentPosition;
                TimeSpan currentDuration;
                if (p1.Playing == p2.Playing)
                {
                    currentPosition = new TimeSpan(0, 0, (int)Math.Max(p1.Position, p2.Position));
                    currentDuration = (p1.Position >= p2.Position) ? new TimeSpan(0, 0, (int)p1.Duration) : new TimeSpan(0, 0, (int)p2.Duration);
                }
                else
                {
                    currentPosition = p1.Playing ? new TimeSpan(0, 0, (int)p1.Position) : new TimeSpan(0, 0, (int)p2.Position);
                    currentDuration = p1.Playing ? new TimeSpan(0, 0, (int)p1.Duration) : new TimeSpan(0, 0, (int)p2.Duration);
                }

                return currentPosition.ToString(@"hh\:mm\:ss") + " / " + currentDuration.ToString(@"hh\:mm\:ss");
            }
        }

        public double Position { get => CurrentPlayer.Position; }

        public double Duration { get => CurrentPlayer.Duration; }

        public List<IndexedFileInfo> Files { get; set; }

        public int PlayingIndex { get; set; } = 0;

        public int SelectedIndex
        {
            get => selectedIndex;
            set => SetProperty(ref selectedIndex, value);
        }

        public IndexedFileInfo SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                RaisePropertyChanged();
            }
        }

        public string PlayingFileName
        {
            get
            {
                if (Files == null || Files.Count <= PlayingIndex)
                {
                    return string.Empty;
                }
                else if (mediaSwitching && Files.Count > PlayingIndex)
                {
                    return Files[PlayingIndex - 1].Name + " > " + Files[PlayingIndex].Name;
                }

                return Files[PlayingIndex].Name;
            }
        }

        /// <summary>
        /// ２つのプレイヤーのうち、どちらか片方だけが終了直前のとき、そのプレイヤーを返します。
        /// 両方のプレイヤーが終了直前、または終了直前ではない場合、null を返します。
        /// </summary>
        private SoundPlayer EitherBeforePlayEnd
        {
            get
            {
                if (players[0].PassedBeforeEndPoint == players[1].PassedBeforeEndPoint)
                {
                    return null;
                }

                return players[0].PassedBeforeEndPoint ? players[0] : players[1];
            }
        }

        /// <summary>
        /// このクラスが保持するプレイヤーの内、Position(現在の再生位置）の値が大きい方のプレイヤーを返却します。
        /// </summary>
        private SoundPlayer CurrentPlayer
        {
            get
            {
                var p1 = players[(int)PlayerIndex.First];
                var p2 = players[(int)PlayerIndex.Second];

                return (p1.Position >= p2.Position) ? p1 : p2;
            }
        }

        public void Play(int index = 0)
        {
            PlayingIndex = index;
            SelectedIndex = PlayingIndex;
            SelectedItem = Files[PlayingIndex];
            RaisePropertyChanged(nameof(PlayingFileName));
            CurrentPlayer.SoundFileInfo = Files[PlayingIndex].FileInfo;
            CurrentPlayer.NewPlayer();
        }

        public void UpdateSelectedFileName() => RaisePropertyChanged(nameof(PlayingFileName));

        public void Stop()
        {
            PlayingIndex = int.MaxValue;
            CurrentPlayer.Stop();
            GetOtherPlayer(CurrentPlayer).Stop();

            CurrentPlayer.Volume = this.Volume;
            GetOtherPlayer(CurrentPlayer).Volume = this.Volume;

            CurrentPlayer.SetEmptyFileInfo();
            UpdateSelectedFileName();
            mediaSwitching = false;
        }

        private void TimerEventHandler()
        {
            RaisePropertyChanged(nameof(PlayTime));
            RaisePropertyChanged(nameof(Position));
            RaisePropertyChanged(nameof(Duration));
            SoundPlayer player = EitherBeforePlayEnd;
            if (player != null)
            {
                // ボリュームの操作等の曲の切り替え中の動作をここに記述する
                SoundPlayer otherPlayer = GetOtherPlayer(player);

                if (autoStoped)
                {
                    // 何度もファイルの読み込みを行わないようにするため、一度自動停止したらここでリターンする。
                    // このフラグが立つタイミングは、ファイル再生の途中となるので、
                    // ファイル再生終了に対するイベントハンドラ内でフラグを初期化する
                    return;
                }

                if (!otherPlayer.Playing)
                {
                    if (Files.Count < PlayingIndex + 1)
                    {
                        return;
                    }

                    PlayingIndex++;
                    SelectedIndex = PlayingIndex;
                    otherPlayer.SoundFileInfo = Files[PlayingIndex].FileInfo;

                    // ここで行う曲の新規再生（切り替え）は BackCut, FrontCut を加味した play() メソッドで行う。
                    otherPlayer.Play();
                    otherPlayer.Volume = 0;
                    otherPlayer.PlayStartedEvent += StopMedia;
                }

                int volumeUpAmount = (SwitchingDuration != 0) ? Volume / SwitchingDuration : 0;
                int volumeDownAmount = (SwitchingDuration != 0) ? Volume / SwitchingDuration : 0;

                if (otherPlayer.Volume + volumeUpAmount < Volume)
                {
                    otherPlayer.Volume += volumeUpAmount;
                }
                else
                {
                    otherPlayer.Volume = Volume;
                }

                player.Volume -= Volume / switchingDuration / 2;
            }
        }

        private void StopMedia(object sender)
        {
            SoundPlayer p = sender as SoundPlayer;
            if (p.Duration <= p.SecondsOfBeforeEndNotice * 2)
            {
                p.Stop();
                PlayingIndex--;
                autoStoped = true;
            }
            else
            {
                // 再生継続の場合は曲名の更新通知を飛ばす
                mediaSwitching = true;
                RaisePropertyChanged(nameof(PlayingFileName));
            }

            p.PlayStartedEvent -= StopMedia;
        }

        /// <summary>
        /// テスト時に PrivateObject 経由でタイマーを止めるために使用
        /// </summary>
        private void StopTimer()
        {
            timer.Stop();
        }

        private void NextPlay(object sender)
        {
            autoStoped = false;
            SoundPlayer p = sender as SoundPlayer;
            var anotherPlayer = GetOtherPlayer(p);

            // 逆側のプレイヤーが再生している状態の場合は、このハンドラ内で play() を呼び出すことは無い
            if (!anotherPlayer.Playing)
            {
                if (Files.Count > PlayingIndex + 1)
                {
                    PlayingIndex++;
                    SelectedIndex = PlayingIndex;
                    p.SoundFileInfo = Files[PlayingIndex].FileInfo;
                    p.NewPlayer();
                }
            }

            mediaSwitching = false;
            RaisePropertyChanged(nameof(PlayingFileName));
        }

        /// <summary>
        /// playersに入っているplayerのうち、引数に指定された方ではない側のプレイヤーを取得します。
        /// </summary>
        /// <param name="soundPlayer"></param>
        private SoundPlayer GetOtherPlayer(SoundPlayer soundPlayer)
        {
            if (soundPlayer != players[(int)PlayerIndex.First] && soundPlayer != players[(int)PlayerIndex.Second])
            {
                throw new ArgumentException("入力された SoundPlayer は、playersに格納されている SoundPlayer ではありません");
            }

            if (players[(int)PlayerIndex.First] != soundPlayer)
            {
                return players[(int)PlayerIndex.First];
            }
            else
            {
                return players[(int)PlayerIndex.Second];
            }
        }
    }
}
