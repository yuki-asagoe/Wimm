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
        //TODO これ別クラスに分けたほうがいい説。DocumentBuilderとか
        //100行をゆうに超える関数を作ってしまったことはすまん　ごめんね
        public static void GenerateFolder(Machine machine,DirectoryInfo scriptFolder,DirectoryInfo docsFolder)
        {
            #region scriptFolder
            File.Create(scriptFolder.FullName + "/initialize.neo.lua").Close();
            File.Create(scriptFolder.FullName + "/definition.neo.lua").Close();
            using(var stream= new StreamWriter(File.Create(scriptFolder.FullName + "/control_map.neo.lua")))
            {
                stream.WriteLine("-- コントロールの操作のマッピングを行います。高度な制御がいるならon_controlを使ってネ");
                stream.WriteLine("-- map(key_array,button_array,action)");
                stream.WriteLine("-- 引数にbuttonsとkeysが与えられます。");
                stream.WriteLine("-- buttons : Votice.XInput.GamepadButtons");
                stream.WriteLine("-- keys : 未定、現在参照できません map関数の入力は受け入れますが何もしません");
            }
            using (var stream = new StreamWriter(File.Create(scriptFolder.FullName + "/on_control.neo.lua")))
            {
                stream.WriteLine("-- 毎制御ごとに呼び出します。以下引数");
                stream.WriteLine("-- {Root Module Name} - StructuredModlues これの名前はマシンDLL依存なんだけどよくないかな");
                stream.WriteLine("-- buttons : Votice.XInput.GamepadButtons - https://github.com/amerkoleci/Vortice.Windows/blob/main/src/Vortice.XInput/GamepadButtons.cs");
                stream.WriteLine("-- gamepad : Vortice.Xinput.Gamepad - https://github.com/amerkoleci/Vortice.Windows/blob/main/src/Vortice.XInput/Gamepad.cs");
                stream.WriteLine("-- input : Wimm.Model.Control.Script.InputSupporter");
                stream.WriteLine("-- wimm : Wimm.Model.Control.Script.WimmFeatureProvider");
            }
            var macroFolder = new DirectoryInfo(scriptFolder + "/macro");
            if (!macroFolder.Exists) { Directory.CreateDirectory(macroFolder.FullName); }
            using (var macros = new Utf8JsonWriter(File.Create(macroFolder + "/macros.json"))) 
            {
                var data = new JsonObject
                {
                    new KeyValuePair<string,JsonNode?>("name",JsonValue.Create("Example Macro")),
                    new KeyValuePair<string,JsonNode?>("lifetime",JsonValue.Create(5.0))
                };
                var example = new JsonObject
                {
                    new KeyValuePair<string, JsonNode?>("1",data)
                };
                example.WriteTo(macros);
            };
            File.Create(macroFolder.FullName + "/1.neo.lua").Close();
            #endregion
            #region docs
            using (var writer = File.Create(docsFolder.FullName + "/Reference.html"))
            {
                writer.Write(System.Text.Encoding.UTF8.GetBytes("<!DOCTYPE html>\n"));
                var document = new XmlDocument();
                var html = document.CreateElement("html");
                var head = document.CreateElement("head");
                {
                    var charset = document.CreateElement("meta");
                    charset.SetAttribute("charset", "UTF-8");
                    charset.IsEmpty = true;
                    var title = document.CreateElement("title");
                    title.AppendChild(document.CreateTextNode($"{machine.Name} スクリプト リファレンス"));
                    head.AppendChild(charset);
                    head.AppendChild(title);
                }
                var body = document.CreateElement("body");
                {
                    var structureTitle = document.CreateElement("h2");
                    structureTitle.AppendChild(document.CreateTextNode("モジュールリスト"));
                    body.AppendChild(structureTitle);
                    XmlElement ConstructListItem(ModuleGroup group,string idPrefix) 
                        // idは最終的にName_nameみたいな_区切りになるはず、_を.に置き換えるだけで呼び出せるような形
                    {
                        var rootLi = document!.CreateElement("li");
                        rootLi.AppendChild(document.CreateTextNode(group.Name));
                        var ul = document.CreateElement("ul");
                        rootLi.AppendChild(ul);
                        foreach(var i in group.Children)
                        {
                            ul.AppendChild(ConstructListItem(i,idPrefix+i.Name+"_"));
                        }
                        foreach (var i in group.Modules)
                        {
                            var li = document.CreateElement("li");
                            var a = document.CreateElement("a");
                            a.SetAttribute("href", "#"+idPrefix + i.Name);
                            a.SetAttribute("class", "module");
                            a.AppendChild(document.CreateTextNode(i.Name));
                            ul.AppendChild(li);
                            li.AppendChild(a);
                        }
                        return rootLi;
                    }
                    var li=ConstructListItem(machine.StructuredModules, "");
                    var ul = document.CreateElement("ul");
                    ul.AppendChild(li);
                    body.AppendChild(ul);
                    var descriptionTitle = document.CreateElement("h2");
                    descriptionTitle.AppendChild(document.CreateTextNode("説明"));
                    void AddDescription(ModuleGroup group,string idPrefix,string namePrefix)
                    //NamePrefixはモジュールの名前を.で連結したやつ、スクリプトからの参照にそのままつかえるように
                    {
                        foreach(var i in group.Children)
                        {
                            AddDescription(i, idPrefix + i.Name + "_", namePrefix + i.Name + ".");
                        }
                        foreach(var i in group.Modules)
                        {
                            var p = document!.CreateElement("p");
                            var h3 = document.CreateElement("h3");
                            h3.SetAttribute("class", "module");
                            h3.SetAttribute("id", idPrefix + i.Name);
                            h3.AppendChild(document.CreateTextNode(namePrefix+i.Name));
                            var featureHeader = document.CreateElement("h4");
                            featureHeader.AppendChild(document.CreateTextNode("機能"));
                            var featureUl = document.CreateElement("ul");
                            XmlNode ConstructFeatureListItem(Feature<Delegate> feature)
                            {
                                var builder = new StringBuilder();
                                builder
                                    .Append(feature.Name)
                                    .Append('(');
                                foreach(var f in feature.Function.Method.GetParameters())
                                {
                                    builder
                                        .Append(f.ParameterType)
                                        .Append(' ')
                                        .Append(f.Name)
                                        .Append(',');
                                }
                                if (builder[^1] == ',') { builder.Remove(builder.Length - 1, 1); }
                                builder.Append(')');
                                var item = document!.CreateElement("li");
                                item.SetAttribute("class", "feature");
                                item.AppendChild(document.CreateTextNode(builder.ToString()));
                                return item;
                            }
                            foreach(var f in i.Features)
                            {
                                featureUl.AppendChild(ConstructFeatureListItem(f));
                            }
                            var descriptionHeader = document.CreateElement("h4");
                            descriptionHeader.InnerText = "説明";
                            p.AppendChild(h3);
                            p.AppendChild(featureHeader);
                            p.AppendChild(featureUl);
                            p.AppendChild(descriptionHeader);
                            p.AppendChild(document.CreateTextNode(i.Description));
                            body!.AppendChild(document.CreateElement("hr"));
                            body!.AppendChild(p);
                        }
                        
                    }
                    AddDescription(machine.StructuredModules, "", "");
                }
                document.AppendChild(html);
                html.AppendChild(head);
                html.AppendChild(body);
                using (var xml = XmlWriter.Create(writer, new XmlWriterSettings() { Encoding = System.Text.Encoding.UTF8 }))
                {
                    document.WriteTo(xml);
                }
            }
            #endregion
        }
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
