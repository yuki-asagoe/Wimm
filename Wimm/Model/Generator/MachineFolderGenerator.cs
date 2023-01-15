using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Wimm.Model.Control;

namespace Wimm.Model.Generator
{
    public static class MachineFolderGenerator
    {
        public static DirectoryInfo? GetMachineRootFolder()
        {
            var exeLocation = Assembly.GetEntryAssembly()?.Location;
            if (exeLocation is null) return null;
            var exe = new FileInfo(exeLocation);
            var exeDirectory = exe.Directory;
            if (exeDirectory is null) return null;
            var path = exeDirectory.FullName + "/Machines";
            var directory = new DirectoryInfo(exeLocation);
            if (directory.Exists) { return directory; }
            else return Directory.CreateDirectory(path);
        }
        public static DirectoryInfo? CreateMachineDirectoryFrom(FileInfo dllFile)
        {
            var root = GetMachineRootFolder();
            if (root is null) return null;
            var dllName = dllFile.Name[..^dllFile.Extension.Length];
            if (dllName is null) return null;
            var machineDirectoryPath = root.FullName + "/" + dllName;
            if (Directory.Exists(machineDirectoryPath)) { return new DirectoryInfo(machineDirectoryPath); }
            var machineDirectory = Directory.CreateDirectory(machineDirectoryPath);
            var scriptDirectory=machineDirectory.CreateSubdirectory("script");
            var docsDirectory = machineDirectory.CreateSubdirectory("docs");
            File.Copy(dllFile.FullName, machineDirectory.FullName + "/" + dllFile.Name, true);
            var machine = MachineController.Builder.GetMachine(dllFile,null);
            using (var description = File.Create(machineDirectory.FullName + "/description.txt")) { };
            using (var metainfo = new Utf8JsonWriter(File.Create(machineDirectory.FullName + "/meta_info.json")))
            {
                var json = new JsonObject();
                json.Add(new KeyValuePair<string, JsonNode?>(
                    "name",JsonValue.Create(machine.Name)
                ));
                json.Add(new KeyValuePair<string, JsonNode?>(
                    "board", JsonValue.Create(machine.ControlBoard)
                ));
                json.WriteTo(metainfo);
            };
            ScriptDriver.GenerateFolder(machine, scriptDirectory, docsDirectory);
            return machineDirectory;
        }
    }
}
