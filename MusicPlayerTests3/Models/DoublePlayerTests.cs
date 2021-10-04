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
                    player.Fader();

                    player.TimerEventHandler();
                }
            }

            Assert.IsFalse(player.Switching);
            Assert.AreEqual(player.PlayingIndex, 0);

            forward(20); // 開始から 20sec
            Assert.IsTrue(player.Switching, "クロスフェード開始");
            Assert.AreEqual(player.PlayingIndex, 1, "曲の切り替えが開始した時点でインデックスは増える");

            forward(1);
            Assert.IsTrue(provider.Sounds[0].Volume < 100);
            Assert.IsTrue(provider.Sounds[1].Volume > 0 && provider.Sounds[1].Volume < 20);

            forward(1);
            Assert.IsTrue(provider.Sounds[0].Volume < 100);
            Assert.IsTrue(provider.Sounds[1].Volume > 0 && provider.Sounds[1].Volume < 40);

            forward(8); // 30sec
            Assert.IsFalse(player.Switching, "クロスフェード終了");
            Assert.AreEqual(player.PlayingIndex, 1, "前に再生した曲が終了する地点。インデックスはそのままの値");

            Assert.AreEqual(provider.Sounds[0].Volume, 0);
            Assert.AreEqual(provider.Sounds[1].Volume, 100);

            forward(10); // 40sec
            Assert.IsTrue(player.Switching, "クロスフェード開始");
            Assert.AreEqual(player.PlayingIndex, 2);

            forward(1);
            Assert.IsTrue(provider.Sounds[1].Volume < 100);
            Assert.IsTrue(provider.Sounds[2].Volume > 0 && provider.Sounds[2].Volume < 20);

            forward(1);
            Assert.IsTrue(provider.Sounds[1].Volume < 100);
            Assert.IsTrue(provider.Sounds[2].Volume > 0 && provider.Sounds[2].Volume < 40);

            forward(8); // 50sec
            Assert.IsFalse(player.Switching, "クロスフェード終了");
            Assert.AreEqual(player.PlayingIndex, 2);

            Assert.AreEqual(provider.Sounds[1].Volume, 0);
            Assert.AreEqual(provider.Sounds[2].Volume, 100);

            forward(10); // 60sec
            Assert.IsTrue(player.Switching, "クロスフェード開始");
            Assert.AreEqual(player.PlayingIndex, 3);

            forward(1);
            Assert.IsTrue(provider.Sounds[2].Volume < 100);
            Assert.IsTrue(provider.Sounds[3].Volume > 0 && provider.Sounds[3].Volume < 20);

            forward(1);
            Assert.IsTrue(provider.Sounds[2].Volume < 100);
            Assert.IsTrue(provider.Sounds[3].Volume > 0 && provider.Sounds[3].Volume < 40);

            forward(8); // 70sec
            Assert.IsFalse(player.Switching, "クロスフェード終了");
            Assert.AreEqual(player.PlayingIndex, 3);

            Assert.AreEqual(provider.Sounds[2].Volume, 0);
            Assert.AreEqual(provider.Sounds[3].Volume, 100);

            forward(10); // 80sec
            Assert.IsTrue(player.Switching, "クロスフェード開始");
            Assert.AreEqual(player.PlayingIndex, 4);

            forward(10); // 90sec
            Assert.IsFalse(player.Switching, "クロスフェード終了");
            Assert.AreEqual(player.PlayingIndex, 4);
        }

        [TestMethod]
        public void StopTest()
        {
            SoundProvider provider = new SoundProvider();

            provider.Sounds.Add(new DummySound() { URL = "a", Duration = 30000 });
            provider.Sounds.Add(new DummySound() { URL = "b", Duration = 30000 });

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
                    player.Fader();

                    player.TimerEventHandler();
                }
            }

            forward(10);
            Assert.AreEqual(player.Sounds.Count, 1, "再生を開始したのでリストの容量は 1");

            player.Stop();
            Assert.AreEqual(player.Sounds.Count, 0, "再生を停止したのでリストの容量は 0");

            player.Play();
            forward(25);
            Assert.AreEqual(player.Sounds.Count, 2, "再生開始後、25秒後、曲のスイッチが始まっているので容量 2");

            //// 複数回連続で呼び出しても問題がないか確認する。

            player.Stop();
            player.Stop();
            player.Stop();

            Assert.AreEqual(player.Sounds.Count, 0, "再生を停止したのでリストの容量は 0");
        }
    }
}