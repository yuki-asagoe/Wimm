using MahApps.Metro.Controls;
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
using System.Windows.Shapes;
using Wimm.Ui.ViewModel;

namespace Wimm.Ui
{
    /// <summary>
    /// MachineFolderRegenerateWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MachineFolderRegenerateWindow : MetroWindow
    {
        RegenerateFolderViewModel ViewModel { get; }
        public MachineFolderRegenerateWindow(RegenerateFolderViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = viewModel;
            InitializeComponent();
        }
        protected override void OnClosed(EventArgs e)
        {
            ViewModel.Dispose();
            base.OnClosed(e);
        }

    }
}
