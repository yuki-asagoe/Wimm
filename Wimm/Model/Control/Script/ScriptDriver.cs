using Neo.IronLua;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using Vortice.XInput;
using Wimm.Machines;
using Wimm.Machines.Component;
using Wimm.Model.Control.Script;
using Wimm.Model.Control.Script.Macro;
using Wimm.Logging;
using System.Text;
using Wimm.Common;
using Wimm.Device;
using Wimm.Model.Control.Script.Device;
using Wimm.Common.Logging;

namespace Wimm.Model.Control
{
    public class ScriptDriver : IDisposable
    {
        Machine Machine { get; init; }
        Lua lua = new Lua();
        int ControllerIndex { get; } = 0;
        string RootModuleName { get; init; }
        ILogger? Logger { get; init; }
        LuaTable ModuleTable { get; init; }
        LuaGlobal ControlEnvironment { get; init; }
        LuaChunk? ControlChunk { get; set; }
        ControlProcess? ControlProcess { get; set; }
        List<KeyBinding> KeyBindings { get; } = new List<KeyBinding>();
        WimmFeatureProvider WimmFeature { get; }
        DeviceFolderLoader DeviceLoader { get; }
        public ImmutableArray<MacroInfo> MacroList { get; private set; }
        public RunningMacro? RunningMacro
        { get; private set; }

        public ScriptDriver(Machine machine, MachineFolder machineFolder, int controllerIndex, WimmFeatureProvider wimmFeature, IntPtr hwnd, ILogger logger)
        {
            var deviceDirectory = DeviceFolder.GetDeviceRootFolder();
            if (deviceDirectory is null)
            {
                throw new IOException("拡張デバイスのフォルダを取得できませんでした。");
            }
            DeviceLoader = new DeviceFolderLoader(deviceDirectory,hwnd,logger);
            WimmFeature = wimmFeature;
            ControllerIndex = controllerIndex;
            Machine = machine;
            ControlEnvironment = lua.CreateEnvironment();
            logger?.Info("Luaマシンモジュールの構築を開始します。");
            ModuleTable = ConstructTableFromGroup(machine.StructuredModules);
            ControlEnvironment.Add(machine.StructuredModules.Name, ModuleTable);
            RootModuleName = machine.StructuredModules.Name;
            Logger = logger;
            #region //Load Machine Folder
            
            ControlEnvironment.DefineFunction("getdevice", this.GetDevice);
            try
            {
                if (machineFolder.ScriptInitializeFile.Exists)
                {
                    ControlEnvironment.DoChunk(
                        machineFolder.ScriptInitializeFile.FullName,
                        new KeyValuePair<string, object>(
                            RootModuleName,
                            ModuleTable
                        )
                    );
                    logger?.Info($"[{machineFolder.ScriptInitializeFile.Name}]ファイルの実行が完了しました");
                }
                else
                {
                    logger?.Warn($"スクリプトファイル[{machineFolder.ScriptInitializeFile.Name}]が見つかりません。スキップしました");
                }
                if (machineFolder.ScriptControlMapFile.Exists)
                {
                    ControlEnvironment.DoChunk(
                        machineFolder.ScriptControlMapFile.FullName,
                        new KeyValuePair<string, object>(
                            "map",
                            (Action<LuaTable, LuaTable, Func<LuaResult>>)MapControl
                        ),
                        new KeyValuePair<string, object>(
                            "buttons",
                            LuaKeysEnum.GamepadKeyEnum
                        ),
                        new KeyValuePair<string, object>(
                            RootModuleName,
                            ModuleTable
                        )
                    );
                    logger?.Info($"[{machineFolder.ScriptControlMapFile.Name}]ファイルの実行が完了しました");
                }
                else
                {
                    logger?.Warn($"スクリプトファイル[{machineFolder.ScriptControlMapFile.Name}]が見つかりません。スキップしました");
                }

                if (machineFolder.ScriptOnControlFile.Exists)
                {
                    ControlChunk = lua.CompileChunk(
                        machineFolder.ScriptOnControlFile.FullName,
                        new LuaCompileOptions(),
                        new KeyValuePair<string, Type>(RootModuleName, typeof(LuaTable)),
                        new KeyValuePair<string, Type>("buttons", typeof(LuaTable)),
                        new KeyValuePair<string, Type>("gamepad", typeof(Gamepad)),
                        new KeyValuePair<string, Type>("wimm", typeof(WimmFeatureProvider)),
                        new KeyValuePair<string, Type>("input", typeof(InputSupporter))
                    );
                    logger?.Info($"[{machineFolder.ScriptOnControlFile.Name}]ファイルのコンパイルが完了しました");
                }
                else
                {
                    logger?.Warn($"スクリプトファイル[{machineFolder.ScriptOnControlFile.Name}]が見つかりません。このロボットにはスクリプトによる定期制御処理は実行されません");
                }
            }
            catch(LuaParseException e)
            {
                StringBuilder builder = new StringBuilder();
                builder
                    .Append(" LuaParseException - ")
                    .AppendLine(e.Message)
                    .AppendLine()
                    .AppendLine($"ファイル名 : {e.FileName}")
                    .AppendLine($"発生行 : {e.Line}");
                FileFormatException newException = new FileFormatException(
                    builder.ToString(),
                    e
                );
                throw newException;
            }
            if (machineFolder.MacroDirectory.Exists)
            {
                logger?.Info("マクロの識別を開始します");
                var serializer = new MacroFolderLoader(machineFolder.MacroDirectory, lua);
                MacroList = serializer.Macros;
                logger?.Info($"マクロの識別が完了しました[総数 : {MacroList.Length}]");
            }
            else
            {
                logger?.Info("マクロフォルダが見つかりませんでした。スキップしました。");
                MacroList = [];
            }
            
            #endregion // Load Machine Folder
        }

        private void MapControl(LuaTable keys,LuaTable buttons,Func<LuaResult> action)
        {
            if(action is null)
            {
                Logger?.Warn($"[{nameof(ScriptDriver)}]対応するLuaアクションがnilであるキーマッピングがあるため、これをスキップしました。");
                return;
            }
            var buttonBits = 0;
            foreach((_,var value) in buttons)
            {
                if (value is int button) buttonBits |= button;
            }
            KeyBindings.Add(new KeyBinding(
                (GamepadButtons)buttonBits,
                action
                ));
        }
        private LuaTable? GetDevice(string id)
        {
            return DeviceLoader.GetDevice(id)?.Item2; 
        }
        public void StartMacro(MacroInfo macro)
        {
            RunningMacro = macro.StartMacro();
        }
        public void StopMacro()
        {
            RunningMacro?.Dispose();
            RunningMacro = null;
        }
        public void ExecuteScriptString(string s)
        {
            var compiled=lua.CompileChunk(
                s, 
                "manual_script",
                new LuaCompileOptions(),
                new KeyValuePair<string, Type>("wimm", typeof(WimmFeatureProvider))
            );
            ControlEnvironment.DoChunk(
                compiled,
                WimmFeature
            );
        }
        public void DoFile(string filePath)
        {
            ControlEnvironment.DoChunk(filePath);
        }
        public void StartControlProcess()
        {
            ControlProcess= Machine.StartControl();
        }
        public void DoControl()
        {
            if(ControlProcess is null)
            {
                throw new InvalidOperationException("制御プロセス外で制御処理を試みました。");
            }
            if (RunningMacro is not null)
            {
                if (RunningMacro.IsRunning){ RunningMacro.Process(ControlEnvironment); }
                else { RunningMacro.Dispose();RunningMacro = null; }
            }
            else if (KeyBindings is not null && XInput.GetState(ControllerIndex, out var state))
            {
                foreach (var bind in KeyBindings)
                {
                    if (bind.IsActive(state.Gamepad)) bind.Action();
                }
                try
                {
                    ControlChunk?.Run(
                        ControlEnvironment,
                        ModuleTable,
                        LuaKeysEnum.GamepadKeyEnum,
                        state.Gamepad,
                        WimmFeature,
                        new InputSupporter()
                    );
                }
                catch (LuaRuntimeException ex)
                {
                    Logger?.Error($"Lua Runtime Error in {ex.FileName}:Line.{ex.Column} : {ex.Message}");
                }
            }
        }
        public bool TryControl()
        {
            if(ControlProcess is null) { return false; }
            DoControl();
            return true;
        }
        public void EndControlProcess()
        {
            ControlProcess?.Dispose();
            ControlProcess = null;
        }
        private static LuaTable ConstructTableFromGroup(ModuleGroup group)
        {
            var table = new LuaTable();
            foreach(var module in group.Modules)
            {
                var value = ConstructTableFromModule(module);
                table[module.Name] = value;
            }
            foreach (var child in group.Children)
            {
                table[child.Name] = ConstructTableFromGroup(child);
            }
            return table;
        }
        private static LuaTable ConstructTableFromModule(Module module)
        {
            var table = new LuaTable();
            foreach(var feature in module.Features)
            {
                table[feature.Name] = feature.Function;
            }
            return table;
        }
        private bool disposedValue;
        public void Dispose()
        {
            if (!disposedValue)
            {
                lua.Dispose();
                DeviceLoader.Dispose();
                GC.SuppressFinalize(this);
                disposedValue = true;
            }
        }
        ~ScriptDriver()
        {
            Dispose();
        }
    }
}
