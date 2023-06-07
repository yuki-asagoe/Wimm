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
using Wimm.Machines;
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
            GenerateScriptFolder(machine, scriptDirectory, docsDirectory);
            return machineDirectory;
        }

        private static void GenerateScriptFolder(Machine machine, DirectoryInfo scriptFolder, DirectoryInfo docsFolder)
        {
            #region scriptFolder
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
                    var li = ConstructListItem(machine.StructuredModules, "");
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
    }
}
