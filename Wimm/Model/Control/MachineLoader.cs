using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Wimm.Common;
using Wimm.Common.Logging;
using Wimm.Device;
using Wimm.Logging;
using Wimm.Machines;
using Wimm.Model.Control.Script;

namespace Wimm.Model.Control
{
    public static class MachineLoader
    {
        public static MachineController Build(MachineFolder machineDirectory, WimmFeatureProvider wimmFeature, IntPtr hwnd, ILogger logger)
        {
            var dll = machineDirectory.MachineAssemblyFile;
            var args = new MachineConstructorArgs(hwnd, new MachineLogger(logger), machineDirectory.MachineDirectory);
            Machine machine = GetMachine(dll, args);
            int gamepadIndex = GeneralSetting.Default.SelectedControllerIndex;
            ScriptDriver binder = new ScriptDriver(machine, machineDirectory, gamepadIndex, wimmFeature, hwnd, logger);
            int contorlInterval = GeneralSetting.Default.ControlInterval;
            return new MachineController(machine, binder, gamepadIndex, contorlInterval);
        }
        public static MachineController Build(DirectoryInfo machineDirectory, WimmFeatureProvider wimmFeature, IntPtr hwnd, ILogger logger)
        {
            return Build(new MachineFolder(machineDirectory), wimmFeature, hwnd, logger);
        }
        public static Machine GetMachine(FileInfo dll, MachineConstructorArgs? args)
        {
            if (!dll.Exists)
            {
                throw new FileNotFoundException(
                    $"\"{dll}\"を検索しましたが、存在しません。"
                );
            }
            var dllAssembly = Assembly.LoadFrom(dll.FullName);
            if (dllAssembly is null)
            {
                throw new FileLoadException(
                    "アセンブリの読み込みに失敗しました。"
                );
            }
            var machineType = GetMachineType(dllAssembly);
            if (machineType is null)
            {
                throw new TypeLoadException(
                    $"Machineクラスの定義が見つかりませんでした。{nameof(LoadTargetAttribute)}が付与されているか等を確認してください。"
                );
            }
            Machine? machine = null;
            machine = GetMachineInstance(machineType, args);
            if (machine is null)
            {
                throw new TypeLoadException(
                    $"型[{machineType.FullName}]に引数({nameof(MachineConstructorArgs)})のコンストラクタが見つかりませんでした。"
                );
            }
            return machine;
        }
        public static Type? GetMachineType(Assembly assembly)
        {
            if (assembly.GetCustomAttribute<LoadTargetAttribute>() is LoadTargetAttribute attr)
            {
                if (attr.Target is Type type && type.IsSubclassOf(typeof(Machine)))
                {
                    return type;
                }
            }
            foreach (var type in assembly.GetTypes())
            {
                if (
                    type.IsSubclassOf(typeof(Machine)) &&
                    type.GetCustomAttribute<LoadTargetAttribute>() is not null
                )
                {
                    return type;
                }
            }
            return null;
        }
        public static Machine? GetMachineInstance(Type machineType)
        {
            if (machineType.IsSubclassOf(typeof(Machine)))
            {
                var constructor = machineType.GetConstructor(Type.EmptyTypes);
                return constructor?.Invoke(null) as Machine;
            }
            return null;
        }
        public static Machine? GetMachineInstance(Type machineType, MachineConstructorArgs? args)
        {
            if (machineType.IsSubclassOf(typeof(Machine)))
            {
                var constructor = machineType.GetConstructor(new Type[] { typeof(MachineConstructorArgs) });
                return constructor?.Invoke(new[] { args }) as Machine;
            }
            return null;
        }
        public static IODevice GetDevice(FileInfo dll, DeviceConstructorArgs? args)
        {
            if (!dll.Exists)
            {
                throw new FileNotFoundException(
                    $"\"{dll}\"を検索しましたが、存在しません。"
                );
            }
            var dllAssembly = Assembly.LoadFrom(dll.FullName);
            if (dllAssembly is null)
            {
                throw new FileLoadException(
                    "アセンブリの読み込みに失敗しました。"
                );
            }
            var deviceType = GetDeviceType(dllAssembly);
            if (deviceType is null)
            {
                throw new TypeLoadException(
                    $"IODeviceクラスの定義が見つかりませんでした。{nameof(LoadTargetAttribute)}が付与されているか等を確認してください。"
                );
            }
            IODevice? device = GetDeviceInstance(deviceType, args);
            if (device is null)
            {
                throw new TypeLoadException(
                    $"型[{deviceType.FullName}]に引数({nameof(MachineConstructorArgs)}?)のコンストラクタが見つかりませんでした。"
                );
            }
            return device;
        }
        public static Type? GetDeviceType(Assembly assembly)
        {
            if (assembly.GetCustomAttribute<LoadTargetAttribute>() is LoadTargetAttribute attr)
            {
                if (attr.Target is Type type && type.IsSubclassOf(typeof(IODevice)))
                {
                    return type;
                }
            }
            foreach (var type in assembly.GetTypes())
            {
                if (
                    type.IsSubclassOf(typeof(IODevice)) &&
                    type.GetCustomAttribute<LoadTargetAttribute>() is not null
                )
                {
                    return type;
                }
            }
            return null;
        }
        public static IODevice? GetDeviceInstance(Type type, DeviceConstructorArgs? args)
        {
            if (type.IsSubclassOf(typeof(IODevice)))
            {
                var constructor = type.GetConstructor(new Type[] { typeof(DeviceConstructorArgs) });
                return constructor?.Invoke(new[] { args }) as IODevice;
            }
            return null;
        }
        public static SortedList<ResourceType, string> GetAdditionalResourceEntries(Assembly assembly)
        {
            var list = new SortedList<ResourceType, string>();
            foreach (var attr in assembly.GetCustomAttributes<BuiltInResourceAttribute>())
            {
                list.Add(attr.Resource, attr.ResourcePath);
            }
            return list;
        }
        public static Stream? GetAdditionalResourceStreamWithPath(Assembly assembly, string path)
        {
            return assembly.GetManifestResourceStream(path);
        }
    }
}
