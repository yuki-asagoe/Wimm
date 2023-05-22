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
    /// ImmersiveControlPage.xaml の相互作用ロジック
    /// </summary>
    public partial class ImmersiveControlPage : Page
    {
        MachineControlViewModel ViewModel { get; }
        public ImmersiveControlPage(MachineControlViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
            DataContext = viewModel;
            Loaded += (_, _) => { Keyboard.Focus(ScreenGrid); };
        }

        private void ScreenGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is >= Key.D0 and <= Key.D9)
            {
                ViewModel.OnImmersiveSelectionItemSelected(e.Key - Key.D0);
            }
        }

        private void ScreenGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.Focus(ScreenGrid);
        }
    }
}
