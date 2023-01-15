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
using System.Windows.Threading;
using Wimm.Ui.ViewModel;

namespace Wimm.Ui
{
    public partial class ControllerSettingPage : Page
    {
        ControllerSettingViewModel ViewModel = new ControllerSettingViewModel();
        DispatcherTimer Timer { get; }
        public ControllerSettingPage()
        {
            DataContext = ViewModel;
            Timer = new DispatcherTimer();
            Timer.Interval = new TimeSpan(0, 0, 3);
            Timer.Tick += Timer_Tick;
            InitializeComponent();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            ViewModel.UpdateState();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var setting = GeneralSetting.Default;
            setting.SelectedControllerIndex = ControllerList.SelectedIndex;
            setting.Save();
        }
    }
}
