﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
using ZXing;

namespace Wimm.Model.Control
{
    public class MachineFolder
    {
        public MachineFolder(DirectoryInfo machineDirectory)
        {
            MachineDirectory = machineDirectory;
            
            MetaInfoFile = new FileInfo($"{machineDirectory.FullName}/meta_info.json");
            ConfigFile = new FileInfo($"{machineDirectory.FullName}/config.json");
            DescriptionFile = new FileInfo($"{machineDirectory.FullName}/description.txt");
            ScriptDirectory = new DirectoryInfo($"{machineDirectory.FullName}/script");
            ScriptInitializeFile = new FileInfo($"{machineDirectory.FullName}/script/initialize.neo.lua");
            if (!ScriptInitializeFile.Exists) { 
                ScriptInitializeFile= new FileInfo($"{machineDirectory.FullName}/script/initialize.lua");
            }
            ScriptControlMapFile = new FileInfo($"{machineDirectory.FullName}/script/control_map.neo.lua");
            if (!ScriptControlMapFile.Exists)
            {
                ScriptControlMapFile = new FileInfo($"{machineDirectory.FullName}/script/control_map.lua");
            }
            ScriptOnControlFile = new FileInfo($"{machineDirectory.FullName}/script/on_control.neo.lua");
            if (!ScriptOnControlFile.Exists)
            {
                ScriptOnControlFile = new FileInfo($"{machineDirectory.FullName}/script/on_control.lua");
            }
            ScriptDefinitionFile = new FileInfo($"{machineDirectory.FullName}/script/definition.neo.lua");
            MacroDirectory = new DirectoryInfo($"{machineDirectory.FullName}/script/macro");
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
            
        }
        public string? _machineName = null;
        public string? _controlSystem = null;
        public FileInfo? _machineAssemblyFile = null;

        public string MachineName 
        {
            get
            {
                if (_machineName is null) LoadMetaInfo();
                return _machineName ?? string.Empty;
            }
        }
        public string ControlSystem
        {
            get
            {
                if (_controlSystem is null) LoadMetaInfo();
                return _controlSystem ?? string.Empty;
            }
        }
        public DirectoryInfo MachineDirectory { get; }
        public DirectoryInfo ScriptDirectory { get; }
        public DirectoryInfo MacroDirectory { get; }
        public FileInfo ScriptInitializeFile { get; }
        public FileInfo ScriptControlMapFile { get; }
        public FileInfo ScriptOnControlFile { get; }
        /// <summary>
        /// Deprecated
        /// </summary>
        public FileInfo ScriptDefinitionFile { get; }
        public FileInfo DescriptionFile { get; }
        public FileInfo MachineAssemblyFile
        {
            get
            {
                if (_machineAssemblyFile is null) LoadMetaInfo();
                return _machineAssemblyFile ??= new FileInfo($"{MachineDirectory.FullName}/{MachineDirectory.Name}.dll");
            }
        }
        public FileInfo ConfigFile { get; }
        public FileInfo MetaInfoFile { get; }
        public FileInfo IconFile { get; }
        public FileInfo MacroRegistryFile { get; }
        public FileInfo[] Macros { get; }

        private void LoadMetaInfo()
        {
            if (MetaInfoFile.Exists)
            {
                using (var stream = MetaInfoFile.OpenRead())
                {
                    var json = JsonNode.Parse(stream);
                    if (json is not null && json["name"] is JsonNode nameNode)
                    {
                        _machineName = nameNode.GetValue<string>();
                    }
                    if (json is not null && json["board"] is JsonNode boardNode)
                    {
                        _controlSystem = boardNode.GetValue<string>();
                    }
                    if (json is not null && json["assembly"] is JsonNode assemblyNode)
                    {
                        _machineAssemblyFile = new FileInfo($"{MachineDirectory.FullName}/{assemblyNode.GetValue<string>()}");
                    }

                }
            }
            _machineName ??= string.Empty;
            _controlSystem ??= string.Empty;
            _machineAssemblyFile ??= new FileInfo($"{MachineDirectory.FullName}/{MachineDirectory.Name}.dll");
        }

        public sealed class Generator : IDisposable
        {
            Machine Machine { get; }
            Assembly MachineAssembly
            {
                get { return Machine.GetType().Assembly; }
            }
            public MachineFolder Folder { get; }
            FileInfo AssemblyFile { get; }
            bool disposed = false;
            SortedList<ResourceType, string>? _resources = null;
            SortedList<ResourceType, string> ResourceEntries
            {
                get
                {
                    return _resources ??= MachineLoader.GetAdditionalResourceEntries(Machine.GetType().Assembly);
                }
            }

            /// <summary>
            /// 既存のマシンフォルダからジェネレータを生成します。
            /// </summary>
            /// <param name="machineDirectory">対象のマシンフォルダ</param>
            public Generator(DirectoryInfo machineDirectory) : this(new MachineFolder(machineDirectory)){}
            public Generator(MachineFolder machineFolder)
            {
                AssemblyFile = machineFolder.MachineAssemblyFile;
                Folder = machineFolder;
                Machine = MachineLoader.GetMachine(Folder.MachineAssemblyFile, null);
            }
            /// <summary>
            /// Dllファイルから新規にジェネレータを生成します。
            /// </summary>
            /// <param name="machineAssemblyFile">対象のマシンDLLファイル</param>
            public Generator(FileInfo machineAssemblyFile)
            {
                AssemblyFile = machineAssemblyFile;
                Machine = MachineLoader.GetMachine(machineAssemblyFile, null);
                var root = GetMachineRootFolders();
                if(root.Length ==0)
                {
                    throw new IOException("マシンフォルダの取得に失敗しました。");
                }
                var dllName = machineAssemblyFile.Name[..^machineAssemblyFile.Extension.Length];
                string? machineDirectoryPath=null;
                foreach (var f in root)
                {
                    var directoryPath = f.FullName + "/" + dllName;
                    if (Directory.Exists(directoryPath))
                    {
                        machineDirectoryPath = directoryPath;
                        break;
                    }
                }
                var machineDirectory = machineDirectoryPath is null ? 
                    GetMachineRootFolder().CreateSubdirectory(dllName) : 
                    new DirectoryInfo(machineDirectoryPath);
                machineDirectory.Create();
                Folder = new MachineFolder(machineDirectory);
                File.Copy(machineAssemblyFile.FullName, Folder.MachineAssemblyFile.FullName, true);
            }
            public Generator GenerateDescription()
            {
                if (disposed) return this;
                using (var description = Folder.DescriptionFile.Create())
                {
                    if (ResourceEntries.TryGetValue(ResourceType.Description, out var path))
                    {
                        using var resource = MachineLoader.GetAdditionalResourceStreamWithPath(MachineAssembly,path);
                        if(resource is not null) { resource.CopyTo(description); }
                    }
                }
                return this;
            }
            public Generator GenerateMetaInfo()
            {
                if (disposed) return this;
                using var stream = Folder.MetaInfoFile.Create();
                using (var metainfo = new Utf8JsonWriter(stream))
                {
                    var json = new JsonObject
                    {
                        new KeyValuePair<string, JsonNode?>(
                            "name", JsonValue.Create(Machine.Name)
                        ),
                        new KeyValuePair<string, JsonNode?>(
                            "board", JsonValue.Create(Machine.ControlSystem)
                        ),
                        new KeyValuePair<string, JsonNode?>(
                            "assembly", JsonValue.Create(AssemblyFile.Name)
                        )
                    };
                    json.WriteTo(metainfo);
                };
                return this;
            }
            public Generator GenerateConfig()
            {
                if (disposed) return this;
                using var stream = Folder.ConfigFile.Create();
                using (var config = new Utf8JsonWriter(stream))
                {
                    Machine.MachineConfig.WriteRegistriesTo(config);
                }
                return this;
            }
            public Generator GenerateDocs()
            {
                if (disposed) return this;
                DirectoryInfo docsFolder = Folder.MachineDirectory.CreateSubdirectory("docs");
                using (var writer = File.Create(docsFolder.FullName + "/Reference.html"))
                {
                    if (ResourceEntries.TryGetValue(ResourceType.Reference,out var path))
                    {
                        using var resource = MachineLoader.GetAdditionalResourceStreamWithPath(MachineAssembly, path);
                        if (resource is not null) { resource.CopyTo(writer); }
                    }
                    else {
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
                }
                return this;
            }
            public Generator GenerateIcon()
            {
                if (disposed) return this;
                if (ResourceEntries.TryGetValue(ResourceType.Icon, out var path)) {
                    if (!path.EndsWith(".png")) return this;
                    using var resource = MachineLoader.GetAdditionalResourceStreamWithPath(MachineAssembly, path);
                    if (resource is not null) 
                    {
                        using var stream = Folder.IconFile.Create();
                        resource.CopyTo(stream); 
                    }
                }
                return this;
            }
            public Generator GenerateScript()
            {
                if (disposed) return this;
                DirectoryInfo scriptFolder = Folder.MachineDirectory.CreateSubdirectory("script");
                using(var stream = Folder.ScriptInitializeFile.Create())
                {
                    if (ResourceEntries.TryGetValue(ResourceType.ScriptInitialize, out var path))
                    {
                        using var resource = MachineLoader.GetAdditionalResourceStreamWithPath(MachineAssembly, path);
                        if (resource is not null) { resource.CopyTo(stream); }
                    }
                }
                using (var stream = Folder.ScriptControlMapFile.Create())
                {
                    if (ResourceEntries.TryGetValue(ResourceType.ScriptControlMap, out var path))
                    {
                        using var resource = MachineLoader.GetAdditionalResourceStreamWithPath(MachineAssembly, path);
                        if (resource is not null) { resource.CopyTo(stream); }
                    }
                    else using(var writer = new StreamWriter(stream))
                    {
                        writer.WriteLine("-- コントロールの操作のマッピングを行います。高度な制御が必要ならon_controlを使ってください");
                        writer.WriteLine("-- map(key_list,button_list,action)");
                        writer.WriteLine("-- 引数にbuttonsとkeysが与えられます。");
                        writer.WriteLine("-- buttons : Votice.XInput.GamepadButtons");
                        writer.WriteLine("-- keys : 未定、現在参照できません map関数の入力は受け入れますが何もしません");
                    }
                }
                using (var stream = Folder.ScriptOnControlFile.Create())
                {
                    if (ResourceEntries.TryGetValue(ResourceType.ScriptOnControl, out var path))
                    {
                        using var resource = MachineLoader.GetAdditionalResourceStreamWithPath(MachineAssembly, path);
                        if (resource is not null) { resource.CopyTo(stream); }
                    }
                    else using (var writer = new StreamWriter(stream))
                    {
                        writer.WriteLine("-- 毎制御ごとに呼び出します。以下引数");
                        writer.WriteLine("-- {Root Module Name} - StructuredModlues これの名前はマシンDLL依存なんだけどよくないかな");
                        writer.WriteLine("-- buttons : Votice.XInput.GamepadButtons - https://github.com/amerkoleci/Vortice.Windows/blob/main/src/Vortice.XInput/GamepadButtons.cs");
                        writer.WriteLine("-- gamepad : Vortice.Xinput.Gamepad - https://github.com/amerkoleci/Vortice.Windows/blob/main/src/Vortice.XInput/Gamepad.cs");
                        writer.WriteLine("-- input : Wimm.Model.Control.Script.InputSupporter");
                        writer.WriteLine("-- wimm : Wimm.Model.Control.Script.WimmFeatureProvider");
                    }
                }

                if (!Folder.MacroDirectory.Exists) { Folder.MacroDirectory.Create(); }
                using var macroFileStream = Folder.MacroRegistryFile.Create();
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
                File.Create(Folder.MacroDirectory.FullName + "/1.neo.lua").Close();
                return this;
            }
            public Generator GenerateAll()
            {
                GenerateDescription();
                GenerateDocs();
                GenerateMetaInfo();
                GenerateConfig();
                GenerateScript();
                GenerateIcon();
                return this;
            }
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
        public static DirectoryInfo[] GetMachineRootFolders()
        {
            var exeLocation = Assembly.GetEntryAssembly()?.Location;
            if (exeLocation is null) return Array.Empty<DirectoryInfo>();
            var exe = new FileInfo(exeLocation);
            var exeDirectory = exe.Directory;
            if (exeDirectory is null) return Array.Empty<DirectoryInfo>();
            var path = exeDirectory.FullName + "/Machines";
            DirectoryInfo[] directories = [GetMachineRootFolder(),exeDirectory.CreateSubdirectory("Machines")];
            return directories;
        }
        public static DirectoryInfo GetMachineRootFolder()
        {
            var appFolder = WimmInfo.GetAppFolder();
            return appFolder.CreateSubdirectory("Machines");
        }
        public static void ExtractToZip(DirectoryInfo source,string destinationFilePath)
        {
            if (File.Exists(destinationFilePath)) File.Delete(destinationFilePath);
            using var arc = ZipFile.Open(destinationFilePath, ZipArchiveMode.Create);
            arc.CreateEntryFromDirectory(source.FullName,source.Name);
        }
    }
    internal static class ZipArchiveExtension
    {
        public static void CreateEntryFromAny(this ZipArchive archive, string sourceName, string entryName = "")
        {
            var fileName = Path.GetFileName(sourceName);
            if (File.GetAttributes(sourceName).HasFlag(FileAttributes.Directory))
            {
                archive.CreateEntryFromDirectory(sourceName, Path.Combine(entryName, fileName));
            }
            else
            {
                archive.CreateEntryFromFile(sourceName, Path.Combine(entryName, fileName), CompressionLevel.Fastest);
            }
        }

        public static void CreateEntryFromDirectory(this ZipArchive archive, string sourceDirName, string entryName = "")
        {
            string[] files = Directory.GetFiles(sourceDirName).Concat(Directory.GetDirectories(sourceDirName)).ToArray();
            foreach (var file in files)
            {
                archive.CreateEntryFromAny(file, entryName);
            }
        }
    }
}
