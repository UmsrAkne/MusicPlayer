using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.model {
    public interface IPlayer {

        event EventHandler mediaEnded;
        event EventHandler mediaStarted;

        void play();
        void stop();
        void pause();
        void resume();
        bool Playing { get; }
        bool Loading { get; }
        String URL { get; set; }
        int Volume { get; set; }
        double Position { get; set; }
        double Duration { get;}
    }
}
