using MusicPlayer.model;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.viewModels
{
    class TreeViewModel :BindableBase
    {
        private string baseDirectoryPath = @"C:\";
        public string BaseDirectoryPath {
            get => baseDirectoryPath;
            set => SetProperty(ref baseDirectoryPath, value);
        }

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
