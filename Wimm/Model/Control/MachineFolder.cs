using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml;
using Wimm.Common;
using Wimm.Machines;
using Wimm.Ui.Records;

namespace Wimm.Model.Control
{
    public class MachineFolder
    {
        public MachineFolder(DirectoryInfo machineDirectory)
        {
            MachineDirectory = machineDirectory;
            
            MetaInfoFile = new FileInfo($"{machineDirectory.FullName}/meta_info.json");
            ConfigFile = new FileInfo($"{machineDirectory.FullName}/config.json");
            ScriptInitializeFile = new FileInfo($"{machineDirectory.FullName}/script/initialize.neo.lua");
            ScriptDefinitionFile = new FileInfo($"{machineDirectory.FullName}/script/definition.neo.lua");
            ScriptMappingFile = new FileInfo($"{machineDirectory.FullName}/script/control_map.neo.lua");
            ScriptOnControlFile = new FileInfo($"{machineDirectory.FullName}/script/on_control.neo.lua");
            MacroRegistryFile = new FileInfo($"{machineDirectory.FullName}/script/macro/macros.json");
            LinkedList<FileInfo> buffer = new();
            for (int i = 1; i <= 100; i++)
            {
                var file = new FileInfo($"{machineDirectory.FullName}/script/macro/{i}.neo.lua");
                if (file.Exists)
                {
                    buffer.AddLast(file);
                }
                else
                {
                    break;
                }
            }
            Macros = buffer.ToArray();
            IconFile = new FileInfo($"{machineDirectory.FullName}/icon.png");
            using (var stream = MetaInfoFile.OpenRead())
            {
                var json = JsonNode.Parse(stream);
                if (json is not null && json["name"] is JsonNode nameNode)
                {
                    MachineName = nameNode.GetValue<string>();
                }
                if (json is not null && json["board"] is JsonNode boardNode)
                {
                    ControlBoard = boardNode.GetValue<string>();
                }
                if (json is not null && json["assembly"] is JsonNode assemblyNode)
                {
                    MachineAssemblyFile = new FileInfo($"{machineDirectory.FullName}/{assemblyNode.GetValue<string>()}");
                }
                else
                {
                    MachineAssemblyFile = new FileInfo($"{machineDirectory.FullName}/{MachineDirectory.Name}.dll");
                }
                MachineName ??= string.Empty;
                ControlBoard ??= string.Empty;
            }
        }
        public string MachineName { get; }
        public string ControlBoard { get; }
        public DirectoryInfo MachineDirectory { get; }
        public FileInfo ScriptInitializeFile { get; }
        public FileInfo ScriptDefinitionFile { get; }
        public FileInfo ScriptMappingFile { get; }
        public FileInfo ScriptOnControlFile { get; }
        public FileInfo MachineAssemblyFile { get; }
        public FileInfo ConfigFile { get; }
        public FileInfo MetaInfoFile { get; }
        public FileInfo IconFile { get; }
        public FileInfo MacroRegistryFile { get; }
        public FileInfo[] Macros { get; }
        public sealed class Generator : IDisposable
        {
            Machine Machine { get; }
            public DirectoryInfo MachineDirectory { get; }
            FileInfo AssemblyFile { get; }
            bool disposed = false;
            /// <summary>
            /// 既存のマシンフォルダからジェネレータを生成します。
            /// </summary>
            /// <param name="machineDirectory">対象のマシンフォルダ</param>
            public Generator(DirectoryInfo machineDirectory) : this(new MachineFolder(machineDirectory)){}
            public Generator(MachineFolder machineFolder)
            {
                AssemblyFile = machineFolder.MachineAssemblyFile;
                MachineDirectory = machineFolder.MachineDirectory;
                var machineAssemblyFile = new FileInfo($"{machineFolder.MachineDirectory.FullName}/{MachineDirectory.Name}.dll");
                Machine = MachineController.Builder.GetMachine(machineAssemblyFile, null);
            }
            /// <summary>
            /// Dllファイルから新規にジェネレータを生成します。
            /// </summary>
            /// <param name="machineAssemblyFile">対象のマシンDLLファイル</param>
            public Generator(FileInfo machineAssemblyFile)
            {
                AssemblyFile = machineAssemblyFile;
                Machine = MachineController.Builder.GetMachine(machineAssemblyFile, null);
                var root = GetMachineRootFolder();
                if(root is null)
                {
                    throw new IOException("マシンフォルダの取得に失敗しました。");
                }
                var dllName = machineAssemblyFile.Name[..^machineAssemblyFile.Extension.Length];
                var machineDirectoryPath = root.FullName + "/" + dllName;
                MachineDirectory = new DirectoryInfo(machineDirectoryPath);
                MachineDirectory.Create();
                File.Copy(machineAssemblyFile.FullName, MachineDirectory.FullName + "/" + machineAssemblyFile.Name, true);
            }
            public Generator GenerateDescription()
            {
                if (disposed) return this;
                using (var description = File.Create(MachineDirectory.FullName + "/description.txt")) { };
                return this;
            }
            public Generator GenerateMetaInfo()
            {
                if (disposed) return this;
                using var stream = File.Create(MachineDirectory.FullName + "/meta_info.json");
                using (var metainfo = new Utf8JsonWriter(stream))
                {
                    var json = new JsonObject();
                    json.Add(new KeyValuePair<string, JsonNode?>(
                        "name", JsonValue.Create(Machine.Name)
                    ));
                    json.Add(new KeyValuePair<string, JsonNode?>(
                        "board", JsonValue.Create(Machine.ControlSystem)
                    ));
                    json.Add(new KeyValuePair<string, JsonNode?>(
                        "assembly", JsonValue.Create(AssemblyFile.Name)
                    ));
                    json.WriteTo(metainfo);
                };
                return this;
            }
            public Generator GenerateConfig()
            {
                if (disposed) return this;
                using var stream = File.Create(MachineDirectory.FullName + "./config.json");
                using (var config = new Utf8JsonWriter(stream))
                {
                    Machine.MachineConfig.WriteRegistriesTo(config);
                }
                return this;
            }
            public Generator GenerateDocs()
            {
                if (disposed) return this;
                DirectoryInfo docsFolder = MachineDirectory.CreateSubdirectory("docs");
                using (var writer = File.Create(docsFolder.FullName + "/Reference.html"))
                {
                    writer.Write(Encoding.UTF8.GetBytes("<!DOCTYPE html>\n"));
                    var document = new XmlDocument();
                    var html = document.CreateElement("html");
                    var head = document.CreateElement("head");
                    {
                        var charset = document.CreateElement("meta");
                        charset.SetAttribute("charset", "UTF-8");
                        charset.IsEmpty = true;
                        var title = document.CreateElement("title");
                        title.AppendChild(document.CreateTextNode($"{Machine.Name} スクリプト リファレンス"));
                        head.AppendChild(charset);
                        head.AppendChild(title);
                    }
                    var body = document.CreateElement("body");
                    {
                        var structureTitle = document.CreateElement("h2");
                        structureTitle.AppendChild(document.CreateTextNode("モジュールリスト"));
                        body.AppendChild(structureTitle);
                        XmlElement ConstructListItem(ModuleGroup group, string idPrefix)
                        // idは最終的にName_nameみたいな_区切りになるはず、_を.に置き換えるだけで呼び出せるような形
                        {
                            var rootLi = document!.CreateElement("li");
                            rootLi.AppendChild(document.CreateTextNode(group.Name));
                            var ul = document.CreateElement("ul");
                            rootLi.AppendChild(ul);
                            foreach (var i in group.Children)
                            {
                                ul.AppendChild(ConstructListItem(i, idPrefix + i.Name + "_"));
                            }
                            foreach (var i in group.Modules)
                            {
                                var li = document.CreateElement("li");
                                var a = document.CreateElement("a");
                                a.SetAttribute("href", "#" + idPrefix + i.Name);
                                a.SetAttribute("class", "module");
                                a.AppendChild(document.CreateTextNode(i.Name));
                                ul.AppendChild(li);
                                li.AppendChild(a);
                            }
                            return rootLi;
                        }
                        var li = ConstructListItem(Machine.StructuredModules, "");
                        var ul = document.CreateElement("ul");
                        ul.AppendChild(li);
                        body.AppendChild(ul);
                        var descriptionTitle = document.CreateElement("h2");
                        descriptionTitle.AppendChild(document.CreateTextNode("説明"));
                        void AddDescription(ModuleGroup group, string idPrefix, string namePrefix)
                        //NamePrefixはモジュールの名前を.で連結したやつ、スクリプトからの参照にそのままつかえるように
                        {
                            foreach (var i in group.Children)
                            {
                                AddDescription(i, idPrefix + i.Name + "_", namePrefix + i.Name + ".");
                            }
                            foreach (var i in group.Modules)
                            {
                                var p = document!.CreateElement("p");
                                var h3 = document.CreateElement("h3");
                                h3.SetAttribute("class", "module");
                                h3.SetAttribute("id", idPrefix + i.Name);
                                h3.AppendChild(document.CreateTextNode(namePrefix + i.Name));
                                var featureHeader = document.CreateElement("h4");
                                featureHeader.AppendChild(document.CreateTextNode("機能"));
                                var featureUl = document.CreateElement("ul");
                                XmlNode ConstructFeatureListItem(Feature<Delegate> feature)
                                {
                                    var builder = new StringBuilder();
                                    builder
                                        .Append(feature.Name)
                                        .Append('(');
                                    foreach (var f in feature.Function.Method.GetParameters())
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
                                foreach (var f in i.Features)
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
                        AddDescription(Machine.StructuredModules, "", "");
                    }
                    document.AppendChild(html);
                    html.AppendChild(head);
                    html.AppendChild(body);
                    using (var xml = XmlWriter.Create(writer, new XmlWriterSettings() { Encoding = System.Text.Encoding.UTF8 }))
                    {
                        document.WriteTo(xml);
                    }
                }
                return this;
            }
            public Generator GenerateScript()
            {
                if (disposed) return this;
                DirectoryInfo scriptFolder = MachineDirectory.CreateSubdirectory("script");
                File.Create(scriptFolder.FullName + "/initialize.neo.lua").Close();
                File.Create(scriptFolder.FullName + "/definition.neo.lua").Close();
                using (var stream = new StreamWriter(File.Create(scriptFolder.FullName + "/control_map.neo.lua")))
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
                using var macroFileStream = File.Create(macroFolder + "/macros.json");
                using (var macros = new Utf8JsonWriter(macroFileStream))
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
                return this;
            }
            public Generator GenerateAll()
            {
                GenerateDescription();
                GenerateDocs();
                GenerateMetaInfo();
                GenerateConfig();
                GenerateScript();
                return this;
            }
            public MachineFolder GetMachineFolder() => new MachineFolder(MachineDirectory);
            public void Dispose()
            {
                if (disposed) return; else Machine?.Dispose();
                disposed = true;
                GC.SuppressFinalize(this);
            }
            ~Generator()
            {
                Dispose();
            }
        }
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
    }
}
