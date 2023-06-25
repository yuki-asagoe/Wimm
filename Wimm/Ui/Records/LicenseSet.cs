using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Wimm.Ui.Records
{
    internal class LicenseSet
    {
        public string MahApps_Metro { get; }
        public string NeoLua { get; }
        public string ObservableCollections { get; }
        public string OpenCVSharp4 { get; }
        public string Simple_Wpf_Terminal { get; }
        public string Vortice_XInput { get; }
        private static LicenseSet? instance = null;
        public static LicenseSet Instance
        {
            get
            {
                instance ??= new LicenseSet();
                return instance;
            }
        }
        private LicenseSet()
        {
            using(var stream=new StreamReader(Application.GetResourceStream(new Uri(@"Resources\Text\MahApps.Metro.License.txt", UriKind.Relative)).Stream))
            {
                MahApps_Metro = stream.ReadToEnd();
            }
            using (var stream = new StreamReader(Application.GetResourceStream(new Uri(@"Resources\Text\NeoLua.License.txt", UriKind.Relative)).Stream))
            {
                NeoLua = stream.ReadToEnd();
            }
            using (var stream = new StreamReader(Application.GetResourceStream(new Uri(@"Resources\Text\ObservableCollections.License.txt", UriKind.Relative)).Stream))
            {
                ObservableCollections = stream.ReadToEnd();
            }
            using (var stream = new StreamReader(Application.GetResourceStream(new Uri(@"Resources\Text\OpenCvSharp4.License.txt", UriKind.Relative)).Stream))
            {
                OpenCVSharp4 = stream.ReadToEnd();
            }
            using (var stream = new StreamReader(Application.GetResourceStream(new Uri(@"Resources\Text\Simple.Wpf.Terminal.License.txt", UriKind.Relative)).Stream))
            {
                Simple_Wpf_Terminal = stream.ReadToEnd();
            }
            using (var stream = new StreamReader(Application.GetResourceStream(new Uri(@"Resources\Text\Vortice.XInput.License.txt", UriKind.Relative)).Stream))
            {
                Vortice_XInput = stream.ReadToEnd();
            }
        }
    }
}
