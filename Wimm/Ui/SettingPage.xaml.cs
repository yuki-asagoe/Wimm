using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wimm.Ui
{
    /// <summary>
    /// SettingPage.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingPage : Page
    {
        GeneralSetting Setting { get; init; }
        public SettingPage()
        {
            Setting = GeneralSetting.Default;
            DataContext = Setting;
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Setting.Save();
            FeedBackText.Text = "変更を保存しました。";
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Setting.Reset();
            FeedBackText.Text = "初期値に設定しました。";
        }
    }
}
