using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Wimm.Model.Control;
using Wimm.Ui.Commands;
using Wimm.Ui.Records;

namespace Wimm.Ui.ViewModel
{
    internal class ManageMachineViewModel : DependencyObject
    {
        public static readonly DependencyProperty MachinesProperty =
            DependencyProperty.Register("Machines", typeof(ObservableCollection<MachineEntry>), typeof(ManageMachineViewModel));
        public static readonly DependencyProperty DevicesProperty =
            DependencyProperty.Register("Devices", typeof(ObservableCollection<DeviceEntry>), typeof(ManageMachineViewModel));
        public ObservableCollection<MachineEntry> Machines
        {
            get { return (ObservableCollection<MachineEntry>)GetValue(MachinesProperty); }
            private set { SetValue(MachinesProperty, value); }
        }
        public ObservableCollection<DeviceEntry> Devices
        {
            get { return (ObservableCollection<DeviceEntry>)GetValue(DevicesProperty); }
            private set { SetValue(DevicesProperty, value); }
        }
        public ICommand RemoveModuleCommand { get; }
        public ICommand OpenFolderCommand { get; }
        public ICommand AddMachineCommand { get; }
        public ICommand AddDeviceCommand { get; }
        public ICommand OpenConfigCommand { get; }
        public ICommand ExportModuleCommand { get; }
        public ICommand OpenGeneratorCommand { get; }
        IDialogCoordinator DialogCoordinator { get; }
        public ManageMachineViewModel(IDialogCoordinator dialog)
        {
            DialogCoordinator = dialog;
            Machines = new ObservableCollection<MachineEntry>(MachineEntry.LoadEntries()??Array.Empty<MachineEntry>());
            Devices = new ObservableCollection<DeviceEntry>(DeviceEntry.LoadEntries() ?? Array.Empty<DeviceEntry>());
            OpenFolderCommand = new ParamsDelegateCommand(
                (arg) =>
                {
                    if(arg is DirectoryInfo info)
                    {
                        Process.Start(new ProcessStartInfo(info.FullName) { UseShellExecute = true });
                    }
                }
            );
            RemoveModuleCommand = new ParamsDelegateCommand(
                async (arg) =>
                {
                    if(arg is MachineEntry machine)
                    {
                        var dialogResult=await DialogCoordinator.ShowMessageAsync(this, "ロボットの削除", $"[{machine.Name}] を削除します。この操作は取り消せません。よろしいですか？", MessageDialogStyle.AffirmativeAndNegative);
                        if(dialogResult is MessageDialogResult.Affirmative)
                        {
                            machine.MachineDirectory.Delete(true);
                            Machines.Remove(machine);
                        }
                    }
                    if(arg is DeviceEntry device)
                    {
                        var dialogResult = await DialogCoordinator.ShowMessageAsync(this, "デバイスの削除", $"[{device.Name}] を削除します。この操作は取り消せません。よろしいですか？", MessageDialogStyle.AffirmativeAndNegative);
                        if(dialogResult is MessageDialogResult.Affirmative)
                        {
                            device.Directory.Delete(true);
                            Devices.Remove(device);
                        }
                    }
                }
            );
            AddMachineCommand = new DelegateCommand(
                async() =>
                {
                    var dialog = new OpenFileDialog()
                    {
                        Title = "マシンの追加・インポート",
                        Filter = "アセンブリファイル (*.dll)|*.dll|マシンファイル (*.wimm-machine)|*.wimm-machine"
                    };
                    if (dialog.ShowDialog() is true)
                    {
                        var fileName = dialog.FileName;
                        if (fileName is null) return;
                        var file = new FileInfo(fileName);
                        if (!file.Exists) return;
                        if (file.Extension == ".dll")
                        {
                            using var generator = new MachineFolder.Generator(file);
                            var task = Task.Run(() =>
                            {
                                try
                                {
                                    generator.GenerateAll();
                                    var directory = file.Directory;
                                    // おまけで同一フォルダ内のDllはコピーしておく (依存している可能性が高い
                                    if (directory is not null)
                                    {
                                        foreach (var f in directory.GetFiles().Where(it => it.Name.EndsWith(".dll") && it.Name != file.Name))
                                        {
                                            File.Copy(f.FullName, generator.MachineDirectory.FullName + "/" + f.Name);
                                        }
                                    }
                                }
                                catch(Exception e)
                                {
                                    return e;
                                }
                                return null;
                            });
                            var controller = await DialogCoordinator.ShowProgressAsync(this, "ロボットの追加", "アセンブリ読み込み中...");
                            controller.SetIndeterminate();
                            var result = await task;
                            await controller.CloseAsync();
                            if(result is null)
                            {
                                Machines = new ObservableCollection<MachineEntry>(MachineEntry.LoadEntries());
                                await DialogCoordinator.ShowMessageAsync(this, "ロボットの追加", "正常に完了しました。");
                            }
                            else
                            {
                                await DialogCoordinator.ShowMessageAsync(this, "エラーが発生しました", $"[{result.GetType().FullName}]\n{result.Message}");
                            }
                        }
                        if(file.Extension == ".wimm-machine")
                        {
                            try
                            {
                                ZipFile.ExtractToDirectory(file.FullName, MachineFolder.GetMachineRootFolder()!.FullName);
                            }
                            catch (Exception e)
                            {
                                await DialogCoordinator.ShowMessageAsync(this, "エラーが発生しました", $"[{e.GetType().FullName}]\n{e.Message}");
                                return;
                            }
                            await DialogCoordinator.ShowMessageAsync(this, "マシンのインポート", "正常に完了しました。");
                        }
                    }
                }
            );
            AddDeviceCommand = new DelegateCommand(
                async() =>
                {
                    var dialog = new OpenFileDialog()
                    {
                        Title = "デバイスの追加・インポート",
                        Filter = "アセンブリファイル (*.dll)|*.dll|デバイスファイル (*.wimm-device)|*.wimm-device"
                    };
                    if (dialog.ShowDialog() is true)
                    {
                        var fileName = dialog.FileName;
                        if (fileName is null) return;
                        var file = new FileInfo(fileName);
                        if (!file.Exists) return;
                        if (file.Extension == ".dll")
                        {
                            using var generator = new DeviceFolder.Generator(file);
                            var task = Task.Run(() =>
                            {
                                try
                                {
                                    generator.GenerateAll();
                                }
                                catch (Exception e)
                                {
                                    return e;
                                }
                                return null;
                            });
                            var controller = await DialogCoordinator.ShowProgressAsync(this, "デバイスの追加", "アセンブリ読み込み中...");
                            controller.SetIndeterminate();
                            var result = await task;
                            await controller.CloseAsync();
                            if (result is null)
                            {
                                Devices = new ObservableCollection<DeviceEntry>(DeviceEntry.LoadEntries() ?? Array.Empty<DeviceEntry>());
                                await DialogCoordinator.ShowMessageAsync(this, "デバイスの追加", "正常に完了しました。");
                            }
                            else
                            {
                                await DialogCoordinator.ShowMessageAsync(this, "エラーが発生しました", $"[{result.GetType().FullName}]\n{result.Message}");
                            }
                        }
                        if(file.Extension == ".wimm-device")
                        {
                            try
                            {
                                ZipFile.ExtractToDirectory(file.FullName, DeviceFolder.GetDeviceRootFolder()!.FullName);
                            }
                            catch(Exception e)
                            {
                                await DialogCoordinator.ShowMessageAsync(this, "エラーが発生しました", $"[{e.GetType().FullName}]\n{e.Message}");
                                return;
                            }
                            await DialogCoordinator.ShowMessageAsync(this, "デバイスのインポート", "正常に完了しました。");
                        }
                    }
                }
            );
            OpenConfigCommand = new ParamsDelegateCommand(
                (arg) =>
                {
                    FileInfo file;
                    string name;

                    if (arg is MachineEntry entry)
                    {
                        file = new FileInfo(entry.MachineDirectory.FullName + "/config.json");
                        name = entry.Name;
                    }
                    else if (arg is DeviceEntry deviceEntry)
                    {
                        file = new FileInfo(deviceEntry.Directory.FullName + "/config.json");
                        name = deviceEntry.Name;
                    }
                    else return;
                    var window = new MachineConfigEditWindow();
                    var parent = Window.GetWindow(this);
                    if (parent is not null)
                    {
                        window.Owner = parent;
                    }
                    window.DataContext = new MachineConfigEditViewModel(name,file);
                    window.ShowDialog();
                }
            );
            ExportModuleCommand = new ParamsDelegateCommand(
                async (arg) =>
                {
                    string destinationFilePath;
                    DirectoryInfo source;
                    if(arg is MachineEntry machineEntry)
                    {
                        source = machineEntry.MachineDirectory;
                        var fileDialog = new SaveFileDialog()
                        {
                            Title = "モジュールのエクスポート",
                            FileName = machineEntry.Name+".wimm-machine",
                            Filter = "マシンファイル (*.wimm-machine)|*.wimm-machine"
                        };
                        if (fileDialog.ShowDialog() is true)
                        {
                            destinationFilePath = fileDialog.FileName;
                            new MachineFolder.Generator(source).GenerateMetaInfo().Dispose();
                        }
                        else return;
                    }
                    else if (arg is DeviceEntry deviceEntry)
                    {
                        source = deviceEntry.Directory;
                        var fileDialog = new SaveFileDialog()
                        {
                            Title = "モジュールのエクスポート",
                            FileName = deviceEntry.Name+".wimm-device",
                            Filter = "デバイスファイル (*.wimm-device)|*.wimm-device"
                        };
                        if (fileDialog.ShowDialog() is true)
                        {
                            destinationFilePath = fileDialog.FileName;
                            new DeviceFolder.Generator(source).GenerateMetaInfo().Dispose();
                        }
                        else return;
                    } else return;
                    try
                    {
                        MachineFolder.ExtractToZip(source, destinationFilePath);

                    }
                    catch(Exception e)
                    {
                        await DialogCoordinator.ShowMessageAsync(this, "エラーが発生しました", $"[{e.GetType().FullName}]\n{e.Message}");
                        return;
                    }
                    await DialogCoordinator.ShowMessageAsync(this, "モジュールのエクスポート", "成功しました。");
                }
            );
            OpenGeneratorCommand = new ParamsDelegateCommand(
                (arg) =>
                {
                    if(arg is MachineEntry entry)
                    {
                        try
                        {
                            var window = new MachineFolderRegenerateWindow(new RegenerateFolderViewModel(entry.MachineDirectory));
                            var parent = Window.GetWindow(this);
                            if (parent is not null)
                            {
                                window.Owner = parent;
                            }
                            window.ShowDialog();
                        }
                        catch (Exception exception)
                        {
                            DialogCoordinator.ShowMessageAsync(
                                this,
                                "エラーが発生しました。",
                                $"[{exception.GetType().Name}]{exception.Message}"
                            );
                        }
                    }
                }
            );
        }
    }
}
