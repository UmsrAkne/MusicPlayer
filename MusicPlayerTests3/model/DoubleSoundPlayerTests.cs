using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicPlayer.Models;
using MusicPlayerTests3.model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.Models.Tests
{
    [TestClass()]
    public class DoubleSoundPlayerTests
    {
        [TestMethod()]
        public void DoubleSoundPlayerTest()
        {

            var wmpA = new DummyWMP();
            var wmpB = new DummyWMP();
            wmpA.NextMediaDuration = 10;
            wmpB.NextMediaDuration = 10;

            var sp1 = new SoundPlayer(wmpA);
            var sp2 = new SoundPlayer(wmpB);

            DoubleSoundPlayer dsp = new DoubleSoundPlayer(sp1, sp2);
            dsp.SwitchingDuration = 10;
            PrivateObject po = new PrivateObject(dsp);
            po.Invoke("stopTimer");

            Action<int> f = (int count) =>
            {
                for (var i = 0; i < count; i++)
                {
                    wmpA.forward();
                    wmpB.forward();

                    if (i % 2 == 0)
                    {
                        // doubleSoundPlayer.timer の間隔が 450ms なので大体ループ２回に１回の頻度。
                        po.Invoke("timerEventHandler");
                    }

                }
            };


            var files = new List<IndexedFileInfo>();
            files.Add(new IndexedFileInfo(new FileInfo("testFile1")));
            files.Add(new IndexedFileInfo(new FileInfo("testFile2")));
            files.Add(new IndexedFileInfo(new FileInfo("testFile3")));
            files.Add(new IndexedFileInfo(new FileInfo("testFile4")));
            files.Add(new IndexedFileInfo(new FileInfo("testFile5")));
            files.Add(new IndexedFileInfo(new FileInfo("testFile6")));
            files.Add(new IndexedFileInfo(new FileInfo("testFile7")));

            foreach (IndexedFileInfo fi in files)
            {
                fi.FileInfo.Create();
            }

            dsp.Files = files;
            dsp.Play();

            Assert.IsTrue(sp1.Playing);
            Assert.IsFalse(sp2.Playing);
            Assert.IsTrue(wmpA.Loading, "wmpAのplay実行直後なのでロード中");


            f(1);
            Assert.IsFalse(wmpA.Loading, "forwardを一回実行したのでロードは終了している");

            bool mediaEndedEventDispatched = false;
            sp1.mediaEndedEvent += (sender) => { mediaEndedEventDispatched = true; };

            f(50);
            Assert.IsTrue(mediaEndedEventDispatched, "50回以上回したらメディアは終了するので、イベントが送出されているはず");

            // 再生中のファイル名は次に移っている
            Assert.AreEqual(sp1.SoundFileInfo.Name, "testFile2");
            Assert.IsTrue(wmpA.Loading);
            Assert.IsFalse(sp2.Playing);

            f(1);
            Assert.IsFalse(wmpA.Loading, "このタイミングではロードが完了している");

            f(1);
            wmpA.NextMediaDuration = 25;
            wmpB.NextMediaDuration = 25;

            f(50);
            Assert.AreEqual(sp1.SoundFileInfo.Name, "testFile3");

            // ループ76回(15秒強経過)で次の曲の再生が始まってほしい
            f(76);
            Assert.IsTrue(wmpA.Playing);
            Assert.IsTrue(wmpB.Playing);

            Assert.AreEqual(Math.Floor(sp1.Position), 15); // 約１５秒目
            Assert.AreEqual(sp2.SoundFileInfo.Name, "testFile4");

            // 更に時間を進めて wmpA の曲が終了する
            f(50);
            Assert.IsFalse(wmpA.Playing);
            Assert.IsTrue(wmpB.Playing);

            // 更に進め、次は時間が短い曲
            // 短い曲の場合は、再生中の方のプレイヤーがそのまま使用される。
            wmpA.NextMediaDuration = 10;
            wmpB.NextMediaDuration = 10;
            f(50);

            // 現在再生されているのは sp2(wmoB)の方。再生ファイル名は以下の通り
            f(60);
            Assert.IsTrue(wmpB.Playing);
            Assert.IsFalse(wmpA.Playing);
            Assert.AreEqual(sp2.SoundFileInfo.Name, "testFile5");
            Assert.AreEqual(sp2.Duration, 10);

            // 再生を行うプレイヤーは据え置きで次のファイルが再生される。
            // Duration は短く設定されているので、フェードは行われない。
            f(60);
            Assert.IsTrue(wmpB.Playing);
            Assert.IsFalse(wmpA.Playing);
            Assert.AreEqual(sp2.SoundFileInfo.Name, "testFile6");
            Assert.AreEqual(sp2.Duration, 10);
            System.Diagnostics.Debug.WriteLine($"{wmpB.Position}");
        }
    }
}