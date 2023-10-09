using Neo.IronLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wimm.Common.Logging;
using Wimm.Device;
using Wimm.Model.Control.Device;

namespace Wimm.Model.Control.Script.Device
{
    internal class DeviceFolderLoader : IDisposable
    {
        SortedList<string, DeviceFolder> Folders { get; } = new SortedList<string, DeviceFolder>();
        SortedList<string, IODevice> LoadedInstances { get; } = new SortedList<string, IODevice>();
        SortedList<string, LuaTable> ConstructedTables { get; } = new SortedList<string, LuaTable>();
        IntPtr HWND { get; }
        ILogger Logger { get; }
        public DeviceFolderLoader(DirectoryInfo deviceRootDirectory,IntPtr hwnd,ILogger logger)
        {
            HWND = hwnd;
            Logger = logger;
            foreach(var directory in deviceRootDirectory.EnumerateDirectories())
            {
                var deviceFolder = new DeviceFolder(directory);
                if (!deviceFolder.IsCorrectFormat) continue;
                Folders[deviceFolder.DeviceID] = deviceFolder;
            }
        }
        public (IODevice,LuaTable)? GetDevice(string id)
        {
            if(LoadedInstances.TryGetValue(id,out IODevice? device) && ConstructedTables.TryGetValue(id,out LuaTable? deviceTable))
            {
                return (device,deviceTable);
            }
            if(Folders.TryGetValue(id,out DeviceFolder? folder))
            {
                device = MachineController.Builder.GetDevice(folder.DeviceAssemblyFile, new DeviceConstructorArgs(HWND,Logger,folder.DeviceDirectory));
                if (device is null) return null;
                return (LoadedInstances[id] = device, ConstructedTables[id] = ConstructTableFromIODevice(device));
            }
            return null;
        }

        private static LuaTable ConstructTableFromIODevice(IODevice device)
        {
            var table = new LuaTable();
            foreach (var feature in device.Features)
            {
                table[feature.Name] = feature.Function;
            }
            return table;
        }
        ~DeviceFolderLoader()
        {
            if (!disposed) Dispose();
        }
        bool disposed = false;
        public void Dispose()
        {
            if (disposed) return;
            foreach((var _,var device) in LoadedInstances)
            {
                device.Dispose();
            }
            GC.SuppressFinalize(this);
            disposed = true;
        }
    }
}
