namespace MusicPlayer.Models.Tests
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MusicPlayer.Models;
    using MusicPlayerTests3.Models;

    [TestClass]
    public class SoundProviderTests
    {
        [TestMethod]
        public void GetSoundTest()
        {
            DummyDB dummyDB = new DummyDB();
            SoundProvider soundProvider = new SoundProvider(dummyDB);

            List<ISound> sounds = new List<ISound>();
            sounds.Add(new DummySound() { URL = "test/dummySound1" });
            sounds.Add(new DummySound() { URL = "test/dummySound2" });
            sounds.Add(new DummySound() { URL = "test/dummySound3" });
            sounds.Add(new DummySound() { URL = "test/dummySound4" });
            sounds.ForEach(s => soundProvider.Sounds.Add(s));

            Assert.AreEqual(soundProvider.GetSound(), sounds[0]);
            Assert.AreEqual(soundProvider.GetSound(), sounds[1]);
            Assert.AreEqual(soundProvider.GetSound(), sounds[2]);
            Assert.AreEqual(soundProvider.GetSound(), sounds[3]);
            Assert.AreEqual(soundProvider.GetSound(), null);
        }
    }
}