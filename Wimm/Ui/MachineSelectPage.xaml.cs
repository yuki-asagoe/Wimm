using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wimm.Ui.Records;
using Wimm.Ui.ViewModel;

namespace Wimm.Ui
{
    /// <summary>
    /// MachineSelectPage.xaml の相互作用ロジック
    /// </summary>
    public partial class MachineSelectPage : Page
    {
        MachineSelectViewModel ViewModel { get; init; }
        public MachineSelectPage()
        {
            ViewModel = new MachineSelectViewModel();
            DataContext = ViewModel;
            InitializeComponent();
            Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.MachineEntries=new System.Collections.ObjectModel.ObservableCollection<MachineEntry>();
            var entries=MachineEntry.LoadEntries();
            if(entries is not null)
            {
                foreach(var i in entries)
                {
                    ViewModel.MachineEntries.Add(i);
                }
            }
        }

        private void RobotEntryList_SelectionChanged(object sender,SelectionChangedEventArgs e)
        {
            ViewModel.SelectedMachine = ViewModel.MachineEntries[RobotEntryList.SelectedIndex];
        }
        private void CanStartControl(object sender,CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewModel.SelectedMachine is not null;
        }
        private async void StartControlExcuted(object sender,ExecutedRoutedEventArgs e)
        {
            if(RuntimeInformation.ProcessArchitecture != Architecture.X86&&ViewModel.SelectedMachine.ControlBoardName == "Tpip3")
            {
                var root = Window.GetWindow(this);
                if(root is MetroWindow metro)
                {
                    var result = await metro.ShowMessageAsync(
                        "Warning",
                        "x86アーキテクチャでは無いプロセスから TPIP 3 制御のロボットを起動しようとしています。\n正常に起動できない可能性がありますがよろしいですか？",
                        MessageDialogStyle.AffirmativeAndNegative
                    );
                    if(result is MessageDialogResult.Negative or MessageDialogResult.Canceled)
                    {
                        return;
                    }
                }
            }
            var window = new MachineControlWindow(new MachineControlViewModel(ViewModel.SelectedMachine.MachineDirectory));
            window.Show();
            e.Handled = true;
        }

        private void OpenMachineFolderExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", ViewModel.SelectedMachine.MachineDirectory.FullName);
        }

        private void OpenMachineConfigExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var window = new MachineConfigEditWindow();
            window.DataContext = new MachineConfigEditViewModel(ViewModel.SelectedMachine.Name,new FileInfo(ViewModel.SelectedMachine.MachineDirectory.FullName+"/config.json"));
            window.ShowDialog();
        }
    }
}
