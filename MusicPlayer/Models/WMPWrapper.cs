﻿namespace MusicPlayer.Models
{
    using System;
    using WMPLib;

    public class WMPWrapper : ISound
    {
        private WindowsMediaPlayer wmp;

        public WMPWrapper()
        {
            wmp = new WindowsMediaPlayer();
            wmp.PlayStateChange += WmpPlayStateChangeEventHandler;
        }

        public event EventHandler MediaEnded;

        public event EventHandler MediaStarted;

        public bool Playing => wmp.playState == WMPPlayState.wmppsPlaying;

        /// <summary>
        /// URLをセットした時点で true になり、
        /// wmp のステータスが WMPPlayState.wmppsPlaying に変化した時点で false になります。
        /// </summary>
        public bool Loading
        {
            get;
            private set;
        }

        public string URL
        {
            get => wmp.URL;
            set
            {
                wmp.URL = value;
                Loading = true;
            }
        }

        public int Volume
        {
            get => wmp.settings.volume;
            set => wmp.settings.volume = value;
        }

        public double Position
        {
            get => wmp.controls.currentPosition;
            set => wmp.controls.currentPosition = value;
        }

        public double Duration { get => wmp.currentMedia.duration; }

        public void Pause()
        {
            wmp.controls.pause();
        }

        public void Resume()
        {
            wmp.controls.play();
        }

        public void Play()
        {
            wmp.PlayStateChange -= WmpPlayStateChangeEventHandler;
            string url = URL; // 一時退避
            wmp.close();

            wmp = new WindowsMediaPlayer();
            wmp.PlayStateChange += WmpPlayStateChangeEventHandler;
            URL = url;
            wmp.settings.volume = Volume;
            wmp.controls.play();
        }

        public void Stop()
        {
            wmp.controls.stop();
        }

        private void WmpPlayStateChangeEventHandler(int newState)
        {
            if (newState == (int)WMPPlayState.wmppsPlaying)
            {
                MediaStarted?.Invoke(this, new EventArgs());
                Loading = false;
            }

            if (newState == (int)WMPPlayState.wmppsMediaEnded)
            {
                MediaEnded?.Invoke(this, new EventArgs());
            }
        }
    }
}