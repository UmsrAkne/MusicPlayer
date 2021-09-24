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
            //// １曲再生テスト

            var dummySoundA = new DummySound() { Duration = 5 };
            var dummySoundB = new DummySound();

            var doublePlayer = new DoublePlayer(dummySoundA, dummySoundB);
            doublePlayer.PlayList.Add(new FileInfo("a"));

            doublePlayer.Play();

            Assert.IsTrue(dummySoundA.Playing);

            dummySoundA.Forward(5.0);
            Assert.IsFalse(dummySoundA.Playing, "５秒経過で再生終了");
        }

        [TestMethod]
        public void PlaySoundsTest()
        {
            //// ２曲再生テスト

            var dummySoundA = new DummySound() { Duration = 5 };
            var dummySoundB = new DummySound();

            var doublePlayer = new DoublePlayer(dummySoundA, dummySoundB);
            doublePlayer.PlayList.Add(new FileInfo("a"));
            doublePlayer.PlayList.Add(new FileInfo("b"));

            doublePlayer.Play();

            Assert.IsTrue(dummySoundA.Playing);

            dummySoundA.Forward(5.0);
            Assert.IsTrue(dummySoundA.Playing, "次の曲を再生している");
            Assert.AreEqual(dummySoundA.URL, new FileInfo("b").FullName);
        }

        [TestMethod]
        public void PlayCrossFadeTest()
        {
            //// 複数曲再生（クロスフェード）

            var dummySoundA = new DummySound() { Duration = 20, SwitchingDuration = 5 };
            var dummySoundB = new DummySound() { Duration = 20, SwitchingDuration = 5 };

            var doublePlayer = new DoublePlayer(dummySoundA, dummySoundB) { SwitchingDuration = 5 };
            doublePlayer.PlayList.Add(new FileInfo("a"));
            doublePlayer.PlayList.Add(new FileInfo("b"));

            doublePlayer.Play();

            Assert.IsTrue(dummySoundA.Playing);

            dummySoundA.Forward(5.0);
            doublePlayer.Fader();

            dummySoundA.Forward(5.0);
            doublePlayer.Fader();

            Assert.IsFalse(doublePlayer.Switching, "この段階では切り替えは始まっていない");

            dummySoundA.Forward(5.0);
            doublePlayer.Fader();

            Assert.IsTrue(doublePlayer.Switching, "Sound の切り替え中");

            dummySoundA.Forward(0.5);
            doublePlayer.Fader();

            Assert.IsTrue(dummySoundA.Volume < 100, "BGM を下げている途中のはずなので Volume は 100 以下");
            Assert.IsTrue(dummySoundB.Volume > 0, "BGM を上げている途中。0以上");

            dummySoundA.Forward(3.5);
            doublePlayer.Fader();

            Assert.IsTrue(dummySoundA.Volume < 100, "BGM を下げている途中のはずなので Volume は 100 以下");
            Assert.IsTrue(dummySoundB.Volume > 0, "BGM を上げている途中。0以上");

            dummySoundA.Forward(1.5);
            doublePlayer.Fader();

            Assert.IsTrue(dummySoundB.Playing, "次の曲を再生している");
            Assert.IsFalse(dummySoundA.Playing, "再生が終了して停止状態");
        }
    }
}