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
using Wimm.Common;
using Wimm.Machines;
using Wimm.Ui.ViewModel;

namespace Wimm.Ui
{
    /// <summary>
    /// OperatorControlPage.xaml の相互作用ロジック
    /// </summary>
    public partial class OperatorControlPage : Page
    {
        MachineControlViewModel ViewModel;
        
        public OperatorControlPage(MachineControlViewModel viewModel)
        {
            DataContext = viewModel;
            ViewModel = viewModel;
            InitializeComponent();
            ModuleGroupTree.ItemsSource = new ModuleGroup?[] { viewModel?.MachineController?.Machine?.StructuredModules };
        }

        private void OnClickSubVideoViewButton(object sender, RoutedEventArgs e)
        {
            var newWindow = new SubCameraViewWindow(ViewModel);
            if(Window.GetWindow(this) is Window parent)
            {
                newWindow.Owner = parent;
            }
            newWindow.Show();
        }
    }
}
