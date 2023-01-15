using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        MachineControlViewModel ViewModel { get; init; }
        public MachineControlWindow(MachineControlViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = viewModel;
            Loaded += OnLoaded;
            Closing += Window_Closing;
        }

        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            ViewModel.Dispose();
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            var hwnd=HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            var task= ViewModel.OnLoad(hwnd, Screen, Dispatcher);
            var controller= await this.ShowProgressAsync("Please Wait...", "ロボット構築中", isCancelable: true);
            controller.SetIndeterminate();
            var result=await task;
            await controller.CloseAsync();
            if(result is not null)
            {
                await this.ShowMessageAsync("エラーが発生しました。", $"{result.GetType().Name}:{result.Message}");
                Close();
            }
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if(Key.D1 <= e.Key && e.Key <= Key.D9 && ViewModel.MachineController?.Machine?.Camera is Camera camera)
            {
                if (e.Key - Key.D1 + 1 <= camera.Channels.Length)
                    camera.Activate(camera.Channels[e.Key - Key.D1], true);
            }
            if(e.Key is Key.LeftShift or Key.RightShift)
            {
                ViewModel.IsControlRunning = !ViewModel.IsControlRunning;
            }
        }
    }
}
