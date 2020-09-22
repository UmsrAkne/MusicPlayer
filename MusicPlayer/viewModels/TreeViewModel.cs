using MusicPlayer.model;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.viewModels
{
    class TreeViewModel : BindableBase, ICurrentDirectorySource
    {
        private string baseDirectoryPath = @"C:\";
        public string BaseDirectoryPath {
            get => baseDirectoryPath;
            set {
                if (!Directory.Exists(value)) {
                    return;
                }

                SetProperty(ref baseDirectoryPath, value);

                // ディレクトリツリーの最上位の部分を生成する
                var md = new MediaDirectory();
                md.FileInfo = new FileInfo(value);
                md.GetChildsCommand.Execute();

                MediaDirectories = new List<MediaDirectory>(new MediaDirectory[] { md });
            }
        }

        public DirectoryInfo CurrentDirectoryInfo => new DirectoryInfo(BaseDirectoryPath);

        private List<MediaDirectory> mediaDirectories = new List<MediaDirectory>();
        public List<MediaDirectory> MediaDirectories {
            get => mediaDirectories;
            private set => SetProperty(ref mediaDirectories, value);
        }

        public object SelectedItem {
            get; set;
        }

        /// <summary>
        /// </summary>
        /// <param name="baseDirectoryPath">このオブジェクトが始点とするディレクトリを入力します。
        /// 無効な値が入力された場合は Cドライブのルートを設定します。</param>
        public TreeViewModel (string baseDirectoryPath) {
            BaseDirectoryPath = (System.IO.Directory.Exists(baseDirectoryPath)) ? baseDirectoryPath : @"C\";
        }
    }
}
