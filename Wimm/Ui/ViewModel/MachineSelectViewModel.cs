using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Wimm.Model.Generator;
using Wimm.Ui.Records;

namespace Wimm.Ui.ViewModel
{
    public class MachineSelectViewModel : DependencyObject
    {
        
        public static DependencyProperty SelectedMachineProperty
            = DependencyProperty.Register("SelectedMachine", typeof(MachineEntry), typeof(MachineSelectViewModel));
        public static DependencyProperty MachineEntriesProperty
            = DependencyProperty.Register("MachineEntries", typeof(ObservableCollection<MachineEntry>), typeof(MachineSelectViewModel));
        public ObservableCollection<MachineEntry> MachineEntries
        {
            get { return (ObservableCollection<MachineEntry>)GetValue(MachineEntriesProperty); }
            set { SetValue(MachineEntriesProperty, value); }
        }
        public MachineEntry SelectedMachine
        {
            get { return (MachineEntry)GetValue(SelectedMachineProperty); }
            set { SetValue(SelectedMachineProperty, value); }
        }
    }
}
