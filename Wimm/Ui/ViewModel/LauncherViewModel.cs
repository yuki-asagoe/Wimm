using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wimm.Machines;

namespace Wimm.Ui.ViewModel
{
    public class LauncherViewModel : DependencyObject
    {
        public ObservableCollection<MenuItem> MenuItems { get; init; }
        public ObservableCollection<MenuItem> OptionMenuItems { get; init; }
        public static DependencyProperty ContentUriProperty
            = DependencyProperty.Register("ContentUri", typeof(Uri), typeof(LauncherViewModel));
        public LauncherViewModel()
        {
            MenuItems = new ObservableCollection<MenuItem>(
                new MenuItem[]
                {
                    new MenuItem("ホーム",PackIconModernKind.Home,new Uri("EntrancePage.xaml",UriKind.Relative)),
                    new MenuItem("ランチャー",PackIconModernKind.RocketRotated45,new Uri("MachineSelectPage.xaml",UriKind.Relative)),
                    new MenuItem("コントローラー",PackIconModernKind.ControllerXbox,new Uri("ControllerSettingPage.xaml",UriKind.Relative))
                }
            );
            OptionMenuItems = new ObservableCollection<MenuItem>(
                new MenuItem[]
                {
                    new MenuItem("ロボット追加",PackIconModernKind.PageUpload,new Uri("AddMachinePage.xaml",UriKind.Relative)),
                    new MenuItem("設定",PackIconModernKind.Settings,new Uri("SettingPage.xaml",UriKind.Relative)),
                    new MenuItem("インフォメーション", PackIconModernKind.InformationCircle,new Uri("InformationPage.xaml",UriKind.Relative))
                }
            );
        }
        public Uri ContentUri
        {
            get { return (Uri)GetValue(ContentUriProperty); }
            set { SetValue(ContentUriProperty, value); }
        }
    }
    public record MenuItem(string Name,PackIconModernKind IconKind,Uri? ContentUri) { }
}
