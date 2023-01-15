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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Wimm.Ui.ViewModel;
using MenuItem = Wimm.Ui.ViewModel.MenuItem;

namespace Wimm.Ui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LauncherWindow : MetroWindow
    {
        LauncherViewModel ViewModel { get; init; }
        public LauncherWindow()
        {
            ViewModel = new LauncherViewModel();
            DataContext = ViewModel;
            ViewModel.ContentUri = new Uri("MachineSelectPage.xaml",UriKind.Relative);
            InitializeComponent();
        }

        private void SideMenu_ItemInvoked(object sender, HamburgerMenuItemInvokedEventArgs args)
        {
            if (args.InvokedItem is MenuItem item)
            {
                if (item.ContentUri is Uri uri)
                {
                    ViewModel.ContentUri = uri;
                }
            }
        }

        private void BackNavigation_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ContentFrame?.CanGoBack??false;
        }

        private void BackNavigation_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ContentFrame.GoBack();
            e.Handled = true;
        }

        private void ForwardNavigation_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ContentFrame?.CanGoForward ?? false;
        }

        private void ForwardNavigation_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ContentFrame.GoForward();
            e.Handled = true;
        }
    }
}
