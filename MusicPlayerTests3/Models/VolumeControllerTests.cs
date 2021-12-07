namespace MusicPlayer.Models.Tests
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MusicPlayerTests3.Models;

    [TestClass]
    public class VolumeControllerTests
    {
        [TestMethod]
        public void FaderTest()
        {
            var volumeController = new VolumeController();
            volumeController.MaxVolume = 1.0f;
            volumeController.SwitchingDuration = 15;
            volumeController.ExecuteCountPerSec = 4;

            var dummySounds = new List<DummySound>()
            {
                new DummySound(),
                new DummySound(),
            };

            dummySounds.ForEach(s =>
            {
                s.Duration = 40;
                s.Volume = 0;
            });

            volumeController.AddPlayingSound(dummySounds[0]);
            dummySounds[0].Play();

            for (int i = 0; i < 4; i++)
            {
                dummySounds[0].Forward(0.25);
                volumeController.Fader();
            }

            Assert.AreNotEqual(dummySounds[0].Volume, 0);

            for (int i = 0; i < 56; i++)
            {
                dummySounds[0].Forward(0.25);
                volumeController.Fader();
            }

            Assert.IsTrue(dummySounds[0].Volume >= 1.0, "再生開始から 15 秒地点で音量は 0 から max まで上昇");

            for (int i = 0; i < 40; i++)
            {
                dummySounds[0].Forward(0.25);
                volumeController.Fader();
            }

            System.Diagnostics.Debug.WriteLine("-----------------");

            /// 更に 10秒経過で 25秒地点。次のサウンドを挿入する。

            dummySounds[1].Volume = 0;
            dummySounds[1].Play();
            volumeController.AddPlayingSound(dummySounds[1]);

            for (int i = 0; i < 30; i++)
            {
                dummySounds[0].Forward(0.25);
                dummySounds[1].Forward(0.25);
                volumeController.Fader();
            }

            Assert.IsTrue(dummySounds[0].Volume < 0.5, "音量 1 -> 0 に減少の途中");
            Assert.AreNotEqual(dummySounds[0].Volume, 1);
            Assert.IsTrue(dummySounds[1].Volume > 0.5, "音量 0 -> 1 に上昇の途中");
            Assert.AreNotEqual(dummySounds[1].Volume, 0);

            for (int i = 0; i < 30; i++)
            {
                dummySounds[0].Forward(0.25);
                dummySounds[1].Forward(0.25);
                volumeController.Fader();
            }

            Assert.AreEqual(dummySounds[0].Volume, 0);
            Assert.AreEqual(dummySounds[1].Volume, 1);
        }
    }
}