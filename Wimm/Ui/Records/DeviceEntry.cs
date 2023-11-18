using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wimm.Model.Control;

namespace Wimm.Ui.Records
{
    public record DeviceEntry(DeviceFolder Folder,ImageSource? Icon)
    {
        public string Name { get; } = Folder.DeviceName;
        public string ID { get; } = Folder.DeviceID;
        public DirectoryInfo Directory { get; } = Folder.DeviceDirectory;
        public bool IsCorrectFormat { get => Folder.IsCorrectFormat; }
        
        public static DeviceEntry[]? LoadEntries()
        {
            DirectoryInfo? root = DeviceFolder.GetDeviceRootFolder();
            if (root is null) return null;
            var list = new LinkedList<DeviceEntry>();
            foreach(var dir in root.GetDirectories())
            {
                try
                {
                    var folder = new DeviceFolder(dir);
                    var icon = folder.IconFile.Exists ?
                        new BitmapImage(new Uri(folder.IconFile.FullName, UriKind.Absolute)) :
                        null;
                    list.AddLast(new DeviceEntry(folder,null));
                }
                catch(FormatException _)
                {
                    continue;
                }
            }
            return list.ToArray();
        }
    }
}
