using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Wimm.Machines.Video;
using Wimm.Ui.ViewModel;

namespace Wimm.Ui
{
    public partial class MachineControlWindow : MetroWindow
    {
        private enum UiMode { None,Normal,Immersive }
        MachineControlViewModel ViewModel { get; init; }
        private UiMode Mode { get; set; }
        public MachineControlWindow(MachineControlViewModel viewModel)
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
            MainFrame.Navigate(new ImmersiveControlPage(ViewModel));
            ViewModel.TerminalController.Post("Immersive モードに切り替えました。");
        }
        public void NavigateToNormalMode()
        {
            if (Mode is UiMode.Normal) return;
            Mode = UiMode.Normal;
            MainFrame.Navigate(new GeneralControlPage(ViewModel));
            ViewModel.TerminalController.Post("Normal モードに切り替えました。");
        }

        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            ViewModel.Dispose();
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            var hwnd=HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            var task= ViewModel.OnLoad(hwnd, Dispatcher);
            var controller= await this.ShowProgressAsync("Please Wait...", "ロボット制御モデル構築中", isCancelable: true);
            controller.SetIndeterminate();
            var result=await task;
            await controller.CloseAsync();
            if(result is not null)
            {
                await this.ShowMessageAsync("エラーが発生しました。", $"{result.GetType().Name}:{result.Message}");
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
    }
}
