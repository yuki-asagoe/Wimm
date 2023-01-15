using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wimm.Model.Generator;

namespace Wimm.Model.Launcher
{
    public record MachineEntry(string Name,string ControlBoardName,DirectoryInfo MachineDirectory,ImageSource? Icon)
    {
        public static MachineEntry[]? LoadEntries()
        {
            var dir = MachineFolderGenerator.GetMachineRootFolder();
            if (dir is null) return null;
            var list = new LinkedList<MachineEntry>();
            foreach(var i in dir.GetDirectories())
            {
                var metainfoFile = new FileInfo(i.FullName + "/meta_info.json");
                if (!metainfoFile.Exists) continue;
                try
                {
                    using (var stream = metainfoFile.OpenRead())
                    {
                        var json = JsonNode.Parse(stream);
                        BitmapImage? icon = null;
                        var iconFile = new FileInfo(i.FullName + "/icon.png");
                        if (iconFile.Exists)
                        {
                            icon = new BitmapImage();
                            icon.BeginInit();
                            icon.UriSource = new Uri(iconFile.FullName, UriKind.Absolute);
                            icon.EndInit();
                            icon.Freeze();
                        }
                        if (json is not null && json["name"] is JsonNode nameNode && json["board"] is JsonNode boardNode)
                        {
                            list.AddLast(
                                new MachineEntry(nameNode.GetValue<string>(), boardNode.GetValue<string>(), i, icon)
                            );
                        }
                    }
                }catch(IOException)
                {
                    continue;
                }
            }
            return list.ToArray();
        }
    }
    
}
