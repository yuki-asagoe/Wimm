using MahApps.Metro.Controls;
using Microsoft.Win32;
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
using MahApps.Metro.Controls.Dialogs;
using Wimm.Ui.ViewModel;
using Wimm.Model.Generator;

namespace Wimm.Ui
{
    /// <summary>
    /// AddMachinePage.xaml の相互作用ロジック
    /// </summary>
    public partial class AddMachinePage : Page
    {
        AddMachineViewModel ViewModel { get; init; }
        public AddMachinePage()
        {
            ViewModel = new AddMachineViewModel();
            DataContext = ViewModel;
            InitializeComponent();
        }

        private void BrowseFileButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title="ファイルを開く";
            dialog.Filter = "DLLファイル(*.dll)|*.dll";
            if (dialog.ShowDialog()??false)
            {
                ViewModel.FileName = dialog.FileName;
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var task= await ViewModel.AddMachine();
            var error = task.Item1;
            var dir = task.Item2;
            if (Window.GetWindow(this) is MetroWindow metro)
            {
                var result = await metro.ShowMessageAsync(
                    error is null ? "Success" : "Error",
                    error ?? "ロボットの追加に成功しました。",
                    style: MessageDialogStyle.AffirmativeAndNegative,
                    settings: new MetroDialogSettings() {
                        NegativeButtonText = "OK",
                        AffirmativeButtonText = "フォルダを開く",
                    }
                ) ;
                if(result == MessageDialogResult.Affirmative && dir is not null)
                {
                    System.Diagnostics.Process.Start("explorer.exe", dir.FullName);
                }
            }
            else
            {
                MessageBox.Show(error ?? "ロボットの追加に成功しました。");
            }
        }
    }
}
