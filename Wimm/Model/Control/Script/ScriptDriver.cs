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
        public ImmutableArray<MacroInfo> MacroList { get; private set; }
        public RunningMacro? RunningMacro
        { get; private set; }
        
        public ScriptDriver(Machine machine,DirectoryInfo machineFolder,int controllerIndex,WimmFeatureProvider wimmFeature,ILogger? logger=null)
        {
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
            var scriptFolder = new DirectoryInfo(machineFolder.FullName + "/script");
            var definitionFile = new FileInfo(scriptFolder.FullName + "/definition.neo.lua");
            var initializeFile = new FileInfo(scriptFolder.FullName + "/initialize.neo.lua");
            var control_mapFile = new FileInfo(scriptFolder.FullName + "/control_map.neo.lua");
            //var logicalMovementFile = new FileInfo(scriptFolder + "/logical_movement.neo.lua");
            var on_controlFile = new FileInfo(scriptFolder.FullName + "/on_control.neo.lua");
            var macroFolder = new DirectoryInfo(scriptFolder.FullName + "/macro");
            if (!(
                definitionFile.Exists &&
                initializeFile.Exists &&
                control_mapFile.Exists
            ))
            {
                throw new FileNotFoundException("初期化に必要なスクリプトファイルが見つかりません。ファイル名を確認してください。");
            }
            ControlEnvironment.DoChunk(initializeFile.FullName);
            logger?.Info("initializeファイルの実行が完了しました");
            ControlEnvironment.DoChunk(definitionFile.FullName);
            logger?.Info("definitionファイルの実行が完了しました");
            ControlEnvironment.DoChunk(
                control_mapFile.FullName,
                new KeyValuePair<string, object>(
                    "map",
                    (Action<LuaTable, LuaTable, Func<LuaResult>>)MapControl
                ),
                new KeyValuePair<string, object>(
                    "buttons",
                    LuaKeysEnum.GamepadKeyEnum
                ),
                new KeyValuePair<string,object>(
                    RootModuleName,
                    ModuleTable
                )
            );
            if (on_controlFile.Exists) ControlChunk = lua.CompileChunk(
                on_controlFile.FullName,
                new LuaCompileOptions(),
                new KeyValuePair<string, Type>(RootModuleName,typeof(LuaTable)),
                new KeyValuePair<string, Type>("buttons",typeof(LuaTable)),
                new KeyValuePair<string, Type>("gamepad",typeof(Gamepad)),
                new KeyValuePair<string, Type>("wimm",typeof(WimmFeatureProvider)),
                new KeyValuePair<string, Type>("input", typeof(InputSupporter))
            );
            logger?.Info("キーマッピングの配置が完了しました");
            logger?.Info("マクロの識別を開始します");
            if (macroFolder.Exists)
            {
                var serializer = new MacroFolderLoader(macroFolder, lua);
                MacroList=serializer.Macros;
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
        public void StartMacro(MacroInfo macro)
        {
            RunningMacro = macro.StartMacro();
        }
        public void StopMacro()
        {
            RunningMacro?.Dispose();
            RunningMacro = null;
        }
        public void DoFile(string filePath)
        {
            ControlEnvironment.DoChunk(filePath);
        }
        public void StartControlProcess()
        {
            ControlProcess= Machine.StartControlProcess();
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
            var motors = new LuaTable();
            var servos = new LuaTable();
            foreach(var module in group.Modules)
            {
                var value = ConstructTableFromModule(module);
                table[module.Name] = value;
                if(module is Motor) { motors[module.Name]=value; }
                else if(module is ServoMotor) { servos[module.Name]=value; }
            }
            foreach (var child in group.Children)
            {
                table[child.Name] = ConstructTableFromGroup(child);
            }
            table["motor"] = motors;
            table["servo"] = servos;
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
