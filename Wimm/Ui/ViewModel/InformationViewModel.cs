using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Wimm.Ui.Commands;

namespace Wimm.Ui.ViewModel
{
    class InformationViewModel :DependencyObject
    {
        public Architecture ProcessorArchitecture
        {
            get { return RuntimeInformation.ProcessArchitecture; }
        }
        public AssemblyName? AssemblyName
        {
            get { return Assembly.GetEntryAssembly()?.GetName(); }
        }
        public ICommand OpenURLCommand { get; } = new ParamsDelegateCommand(
            (arg) =>
            {
                if(arg is string url && (url.StartsWith("https://")||url.StartsWith("http://")))
                {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
            }
        );
    }
}
