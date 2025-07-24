using System;
using System.Collections;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Wimm.Logging;
using Wimm.Machines.Video;
using Wimm.Ui.ViewModel;

namespace Wimm.Ui
{
    public partial class MachineControlWindow : MetroWindow
    {
        private enum UiMode { None,Normal,Immersive,Operator }
        private ImmersiveControlPage? immersiveUI = null;
        private GeneralControlPage? normalUI = null;
        private OperatorControlPage? operatorUI = null;
        private ImmersiveControlPage GetImmersiveControlPage(MachineControlViewModel viewModel)
        {
            immersiveUI ??= new ImmersiveControlPage(ViewModel);
            return immersiveUI;
        }
        private GeneralControlPage GetGeneralControlPage(MachineControlViewModel viewModel)
        {
            normalUI ??= new GeneralControlPage(viewModel);
            return normalUI;
        }
        private OperatorControlPage GetOperatorControlPage(MachineControlViewModel viewModel)
        {
            operatorUI ??= new OperatorControlPage(viewModel);
            return operatorUI;
        }
        MachineControlViewModel ViewModel { get; init; }
        private UiMode Mode { get; set; }
        internal MachineControlWindow(MachineControlViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = viewModel;
            Loaded += OnLoaded;
            Closing += Window_Closing;
        }
        public void NavigateToImmersiveMode()
        {
            if (Mode is UiMode.Immersive) return;
            Mode = UiMode.Immersive;
            MainFrame.Navigate(GetImmersiveControlPage(ViewModel));
            ViewModel.TerminalController.Post("Immersive モードに切り替えました。");
        }
        public void NavigateToNormalMode()
        {
            if (Mode is UiMode.Normal) return;
            Mode = UiMode.Normal;
            MainFrame.Navigate(GetGeneralControlPage(ViewModel));
            ViewModel.TerminalController.Post("Normal モードに切り替えました。");
        }
        public void NavigateToOperatorMode()
        {
            if (Mode is UiMode.Operator) return;
            Mode = UiMode.Operator;
            MainFrame.Navigate(GetOperatorControlPage(ViewModel));
            ViewModel.TerminalController.Post("Operator モードに切り替えました。");
        }

        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            ViewModel.Dispose();
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            var hwnd=new WindowInteropHelper(this).Handle;
            var task= ViewModel.OnLoad(hwnd, Dispatcher);
            var controller= await this.ShowProgressAsync("Please Wait...", "ロボット制御モデル構築中", isCancelable: true);
            controller.SetIndeterminate();
            var result=await task;
            await controller.CloseAsync();
            if(result is not null)
            {
                var errorMessageTask=this.ShowMessageAsync("エラーが発生しました", $"{result.GetType().Name}:{result.Message}");
                ViewModel.TerminalController.Post("ロボット制御モデルの初期化中にエラーが発生しました");
                ViewModel.TerminalController.Post(CrashReport.getDetailedExceptionString(result));
                await errorMessageTask;
                Close();
            }
            else
            {
                NavigateToNormalMode();
            }
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {

        }

        private void OnClickImmersiveButton(object sender, RoutedEventArgs e)
        {
            NavigateToImmersiveMode();
        }

        private void OnClickNormalButton(object sender, RoutedEventArgs e)
        {
            NavigateToNormalMode();
        }

        private void OnClickOperatorButton(object sender, RoutedEventArgs e)
        {
            NavigateToOperatorMode();
        }
    }
}
