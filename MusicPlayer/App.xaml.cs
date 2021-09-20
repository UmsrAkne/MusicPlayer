using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Prism.Unity;
using MusicPlayer.views;
using MusicPlayer.viewModels;

namespace MusicPlayer
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App
    {

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            /* RegisterForNavigation<t,t>() を使うと IDialogService を引数とする MainWIndowViewModel のコンストラクタを呼び出せる。
               呼び出せるが、なぜ呼び出せるのか全くわからない。
            */
            containerRegistry.RegisterForNavigation<MainWindow, MainWindowViewModel>();

            // ビューとビューモデルを登録
            containerRegistry.RegisterDialog<SettingWindow, SettingWindowViewModel>();
            containerRegistry.RegisterDialog<HistoryWindow, HistoryWindowViewModel>();
        }
    }
}
