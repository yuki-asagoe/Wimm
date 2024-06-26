using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wimm.Ui.ViewModel;
using System.Collections.Immutable;

namespace Wimm.Model.Control.Script
{
    public class WimmFeatureProvider
    {
        MachineControlViewModel ViewModel { get; }
        internal WimmFeatureProvider(MachineControlViewModel viewModel)
        {
            ViewModel = viewModel;
        }
        public void SetSpeedModifier(double value)
        {
            value = Math.Clamp(value, 0, 1);
            ViewModel.Dispatcher.BeginInvoke(() => { ViewModel.MachineSpeedModifier = value; });
        }
        public void PostMessage(string text)
        {
            ViewModel.TerminalController.Post("[Script]" + text);
        }
        public void StartMacro(int number)
        {
            var list = ViewModel.MachineController?.MacroList;
            if (list.HasValue)
            {
                if (number < 0 && list.GetValueOrDefault().Length <= number) return;
                ViewModel.Dispatcher.BeginInvoke(() => { ViewModel.StartMacro(list.GetValueOrDefault()[number]); });
            }
        }
        public bool IsCameraActive(int number)
        {
            return ViewModel.Dispatcher.Invoke(() =>
            {
                if (number < 0 && number >= ViewModel.CameraChannelEntries.Count) return false;
                return ViewModel.CameraChannelEntries[number].IsActive;
            });
        }
    }
}
