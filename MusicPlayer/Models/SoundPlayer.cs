namespace MusicPlayer.Models
{
    using System;
    using System.IO;

    public delegate void MediaEndedEventHandler(object sender);

    public delegate void PlayStartedEventHandler(object sender);

    public class SoundPlayer
    {
        private FileInfo soundFileInfo;
        private ISound player;

        public SoundPlayer(ISound player)
        {
            this.player = player;
            this.player.Volume = 100;

            this.player.MediaEnded += (sender, e) =>
            {
                MediaEndedEvent?.Invoke(this);
            };

            this.player.MediaStarted += (sender, e) =>
            {
                Duration = this.player.Duration;
                PlayStartedEvent?.Invoke(this);
            };
        }

        public event MediaEndedEventHandler MediaEndedEvent;

        public event PlayStartedEventHandler PlayStartedEvent;

        public int Volume
        {
            get => player.Volume;

            set
            {
                if (value > 100)
                {
                    player.Volume = 100;
                }
                else if (value < 0)
                {
                    player.Volume = 0;
                }
                else
                {
                    player.Volume = value;
                }
            }
        }

        public double Position
        {
            get => player.Position;
            set => player.Position = value;
        }

        public bool Loading => player.Loading;

        public double Duration { get; private set; } = 0;

        public bool Playing
        {
            get => player.Playing;
            private set
            {
            }
        }

        /// <summary>
        /// PassedBeforeEndPoint 実行時、メディアが終了直前と判定されるポイントを指定します。
        /// </summary>
        public int SecondsOfBeforeEndNotice
        {
            get; set;
        }

        /// <summary>
        /// このメディアが終了直前のポイントを通り過ぎたかどうかを取得します。
        /// 終了直前のポイントとは、"Duration - SecondsOfBeforeEndNotice" の値が示す時点です。
        /// </summary>
        public bool PassedBeforeEndPoint
        {
            get
            {
                if (Duration == 0
                    || SecondsOfBeforeEndNotice * 2 > Duration
                    || !Playing
                    || !BeforeEndPointPassageNotification)
                {
                    return false;
                }

                return Position >= Duration - SecondsOfBeforeEndNotice - BackCut;
            }
        }

        /// <summary>
        /// このメディアの実際の Duration から BackCut を引いた時点を通過したかどうかを取得します。
        /// このプロパティが true を返す場合、実質的にメディアの再生が終了していることを意味します。
        /// </summary>
        public bool PssedBackCutPoint
        {
            get => (Position >= Duration - BackCut);
        }

        /// <summary>
        /// 曲の先頭部分を指定秒数カットします。
        /// </summary>
        public int FrontCut { get; set; }

        /// <summary>
        /// 曲の終端部分を指定秒数カットします。
        /// </summary>
        public int BackCut
        {
            get; set;
        }

        /// <summary>
        /// falseに設定すると、PassedBeforeEndPoint が常に false を返すようになり、
        /// 結果的に クロスフェード機能を無効にします。
        /// </summary>
        public bool BeforeEndPointPassageNotification { get; set; } = true;

        public FileInfo SoundFileInfo
        {
            get => soundFileInfo;
            set
            {
                if (!value.Exists)
                {
                    throw new System.ArgumentException("指定されたファイルが存在しません " + value.FullName);
                }

                soundFileInfo = value;
            }
        }

        /// <summary>
        /// 新規で再生を開始する際に使用します。
        /// </summary>
        public void NewPlayer()
        {
            player.URL = soundFileInfo.FullName;
            player.Play();

            /** ここで情報を記録するので、再生中のメディアが終了N秒前に入ってこのメソッドが呼び出されると、
             *  最後に再生していたメディアが更新される。
             *  最後の方まで再生した曲は、視聴を終えたとみなす。
             */
            Properties.Settings.Default.lastPlayingFileName = SoundFileInfo.FullName;
            Properties.Settings.Default.Save();

            var logFileName = "playlog.txt";
            var text = File.Exists(logFileName) ? File.ReadAllText(logFileName) : string.Empty;

            using (StreamWriter sw = new StreamWriter(logFileName, false))
            {
                sw.WriteLine($"{DateTime.Now.ToString("yyy/MM/dd HH:mm:ss")},{SoundFileInfo.FullName}");
                sw.Write(text);
            }
        }

        /// <summary>
        /// FrontCut の値だけ、冒頭をカットして再生を開始します。
        /// </summary>
        public void Play()
        {
            NewPlayer();

            if (BeforeEndPointPassageNotification)
            {
                // クロスフェードによる音量の変更を伴う場合にのみ FrontCut を有効にする。
                player.Position = FrontCut;
            }
        }

        public void Pause()
        {
            player.Pause();
        }

        public void Resume()
        {
            player.Resume();
        }

        public void Stop()
        {
            player.Stop();
        }

        /// <summary>
        /// SoundFileInfo に対して、空の FileInfo を直接セットします。
        /// </summary>
        public void SetEmptyFileInfo()
        {
            soundFileInfo = new FileInfo("notExistEmptyFileInfo");
        }
    }
}
