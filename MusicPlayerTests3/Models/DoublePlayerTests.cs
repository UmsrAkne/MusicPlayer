using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicPlayer.Models;

namespace MusicPlayer.Models.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MusicPlayer.Models;
    using MusicPlayerTests3.Models;

    [TestClass]
    public class DoublePlayerTests
    {
        [TestMethod]
        public void PlayTest()
        {
            SoundProvider provider = new SoundProvider();

            provider.Sounds.Add(new DummySound() { URL = "a", Duration = 30000 });
            provider.Sounds.Add(new DummySound() { URL = "b", Duration = 30000 });
            provider.Sounds.Add(new DummySound() { URL = "c", Duration = 30000 });
            provider.Sounds.Add(new DummySound() { URL = "d", Duration = 30000 });
            provider.Sounds.Add(new DummySound() { URL = "e", Duration = 30000 });

            DoublePlayer player = new DoublePlayer(provider);
            player.SwitchingDuration = 10;

            player.Play();

            void forward(int count)
            {
                for (var i = 0; i < count; i++)
                {
                    provider.Sounds.ForEach(s => ((DummySound)s).Forward(250));
                    player.Fader();
                    provider.Sounds.ForEach(s => ((DummySound)s).Forward(250));
                    player.Fader();
                    provider.Sounds.ForEach(s => ((DummySound)s).Forward(250));
                    player.Fader();
                    provider.Sounds.ForEach(s => ((DummySound)s).Forward(250));

                    player.TimerEventHandler();
                }
            }

            Assert.IsFalse(player.Switching);
            Assert.AreEqual(player.PlayingIndex, 0);

            forward(20); // 開始から 20sec
            Assert.IsTrue(player.Switching, "クロスフェード開始");
            Assert.AreEqual(player.PlayingIndex, 1, "曲の切り替えが開始した時点でインデックスは増える");

            forward(10); // 30sec
            Assert.IsFalse(player.Switching, "クロスフェード終了");
            Assert.AreEqual(player.PlayingIndex, 1, "前に再生した曲が終了する地点。インデックスはそのままの値");

            forward(10); // 40sec
            Assert.IsTrue(player.Switching, "クロスフェード開始");
            Assert.AreEqual(player.PlayingIndex, 2);

            forward(10); // 50sec
            Assert.IsFalse(player.Switching, "クロスフェード終了");
            Assert.AreEqual(player.PlayingIndex, 2);

            forward(10); // 60sec
            Assert.IsTrue(player.Switching, "クロスフェード開始");
            Assert.AreEqual(player.PlayingIndex, 3);

            forward(10); // 70sec
            Assert.IsFalse(player.Switching, "クロスフェード終了");
            Assert.AreEqual(player.PlayingIndex, 3);

            forward(10); // 80sec
            Assert.IsTrue(player.Switching, "クロスフェード開始");
            Assert.AreEqual(player.PlayingIndex, 4);

            forward(10); // 90sec
            Assert.IsFalse(player.Switching, "クロスフェード終了");
            Assert.AreEqual(player.PlayingIndex, 4);
        }
    }
}