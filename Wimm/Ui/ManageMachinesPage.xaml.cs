using MahApps.Metro.Controls.Dialogs;
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
using Wimm.Ui.ViewModel;

namespace Wimm.Ui
{
    /// <summary>
    /// ManageMachinesPage.xaml の相互作用ロジック
    /// </summary>
    public partial class ManageMachinesPage : Page
    {
        ManageMachineViewModel ViewModel { get; } = new ManageMachineViewModel(DialogCoordinator.Instance);
        public ManageMachinesPage()
        {
            DataContext = ViewModel;
            InitializeComponent();
        }
    }
}
