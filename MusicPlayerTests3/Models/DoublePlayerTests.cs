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
            doublePlayer.PlayList.Add(new FileInfo("b"));

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
    }
}