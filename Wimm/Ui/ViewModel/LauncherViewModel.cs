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
                    new MenuItem("ホーム",PackIconModernKind.Home,new Uri("pack://application:,,,/Wimm;component/Ui/XAML/Page/EntrancePage.xaml",UriKind.Absolute)),
                    new MenuItem("ランチャー",PackIconModernKind.RocketRotated45,new Uri("pack://application:,,,/Wimm;component/Ui/XAML/Page/MachineSelectPage.xaml",UriKind.Absolute)),
                    new MenuItem("コントローラー",PackIconModernKind.ControllerXbox,new Uri("pack://application:,,,/Wimm;component/Ui/XAML/Page/ControllerSettingPage.xaml",UriKind.Absolute)),
                    new MenuItem("モジュール管理",PackIconModernKind.List,new Uri("pack://application:,,,/Wimm;component/Ui/XAML/Page/ManageMachinesPage.xaml",UriKind.Absolute))
                }
             );
            OptionMenuItems = new ObservableCollection<MenuItem>(
                new MenuItem[]
                {
                    new MenuItem("設定",PackIconModernKind.Settings,new Uri("pack://application:,,,/Wimm;component/Ui/XAML/Page/SettingPage.xaml",UriKind.Absolute)),
                    new MenuItem("インフォメーション", PackIconModernKind.InformationCircle,new Uri("pack://application:,,,/Wimm;component/Ui/XAML/Page/InformationPage.xaml",UriKind.Absolute))
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
