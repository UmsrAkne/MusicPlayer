using MusicPlayer.Models;
using Prism.Commands;
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
        public string BaseDirectoryPath
        {
            get => baseDirectoryPath;
            set
            {
                if (!Directory.Exists(value))
                {
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

        public DirectoryInfo CurrentDirectoryInfo => new DirectoryInfo(SelectedItem.FileInfo.FullName);

        private List<MediaDirectory> mediaDirectories = new List<MediaDirectory>();
        public List<MediaDirectory> MediaDirectories
        {
            get => mediaDirectories;
            private set => SetProperty(ref mediaDirectories, value);
        }

        private MediaDirectory selectedItem;
        public MediaDirectory SelectedItem
        {
            get => selectedItem;
            private set
            {
                SetProperty(ref selectedItem, value);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="baseDirectoryPath">このオブジェクトが始点とするディレクトリを入力します。
        /// 無効な値が入力された場合は Cドライブのルートを設定します。</param>
        public TreeViewModel(string baseDirectoryPath)
        {
            BaseDirectoryPath = (System.IO.Directory.Exists(baseDirectoryPath)) ? baseDirectoryPath : @"C\";
        }

        private DelegateCommand<MediaDirectory> selectDirectoryCommand;
        public DelegateCommand<MediaDirectory> SelectDirectoryCommand
        {
            get => selectDirectoryCommand ?? (selectDirectoryCommand = new DelegateCommand<MediaDirectory>(
                (MediaDirectory mediaDirectory) =>
                {
                    SelectedItem = mediaDirectory;
                    Properties.Settings.Default.lastVisitedDirectoryPath = mediaDirectory.FileInfo.FullName;
                    Properties.Settings.Default.Save();
                }
            ));
        }

        /// <summary>
        /// BaseDirectory から、指定したパスに含まれるディレクトリまでが展開された状態のリストを生成し、
        /// MediaDirectories にセットします。
        /// </summary>
        /// <param name="path">パスが存在しない、または BaseDirectory と同一の場合は、BaseDirectoryだけを展開して動作を終了します。</param>
        public void expandItemsTo(string path)
        {
            if (!Directory.Exists(path) || path == BaseDirectoryPath)
            {
                SelectedItem = new MediaDirectory();
                SelectedItem.FileInfo = new FileInfo(BaseDirectoryPath);
                SelectedItem.GetChildsCommand.Execute();
                SelectedItem.IsExpanded = true;
                MediaDirectories = new List<MediaDirectory>(new MediaDirectory[] { SelectedItem });
                return;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            List<DirectoryInfo> directoryInfoList = new List<DirectoryInfo>();

            while (directoryInfo != null)
            {
                directoryInfoList.Add(directoryInfo);
                directoryInfo = directoryInfo.Parent;
                if (directoryInfo.FullName == new DirectoryInfo(BaseDirectoryPath).Parent.FullName)
                {
                    break;
                }
            }

            // ルートに近いディレクトリほど奥に入っていて、ループ処理が逆順になってしまうため逆転させる。
            directoryInfoList.Reverse();

            MediaDirectory md = new MediaDirectory();
            List<MediaDirectory> mdList = new List<MediaDirectory>();
            mdList.Add(md);

            for (var i = 0; i < directoryInfoList.Count; i++)
            {
                md.FileInfo = new FileInfo(directoryInfoList[i].FullName);
                md.GetChildsCommand.Execute();
                md.IsExpanded = true;

                if (directoryInfoList.Count <= i + 1)
                {
                    break;
                }

                md = md.ChildDirectory.FirstOrDefault(m => m.FileInfo.FullName == directoryInfoList[i + 1].FullName);
            }

            md.IsExpanded = true;
            md.IsSelected = true;
            SelectedItem = md;
            MediaDirectories = mdList;
        }

    }
}
