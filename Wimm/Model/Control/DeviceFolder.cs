using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Wimm.Device;

namespace Wimm.Model.Control
{
    public class DeviceFolder
    {
        public DirectoryInfo DeviceDirectory { get; }
        public FileInfo MetaInfoFile { get; }
        public FileInfo ConfigFile { get; }
        public FileInfo DeviceAssemblyFile { get; }
        public FileInfo IconFile { get; }
        public string DeviceName { get; }
        public string DeviceID { get; }

        public DeviceFolder(DirectoryInfo deviceDirectory)
        {
            DeviceDirectory = deviceDirectory;
            MetaInfoFile= new FileInfo($"{deviceDirectory.FullName}/meta_info.json");
            ConfigFile = new FileInfo($"{deviceDirectory.FullName}/config.json");
            IconFile = new FileInfo($"{deviceDirectory.FullName}/icon.png");
            try
            {
                using var stream = MetaInfoFile.OpenRead();
                var json = JsonNode.Parse(stream);
                if (json is not null && json["name"] is JsonNode nameNode)
                {
                    DeviceName = nameNode.GetValue<string>();
                }
                if (json is not null && json["id"] is JsonNode idNode)
                {
                    DeviceID = idNode.GetValue<string>();
                }
                if(json is not null && json["assembly"] is JsonNode assemblyNode)
                {
                    DeviceAssemblyFile = new FileInfo($"{deviceDirectory.FullName}/{assemblyNode.GetValue<string>()}");
                }
                else
                {
                    DeviceAssemblyFile = new FileInfo($"{deviceDirectory.FullName}/{deviceDirectory.Name}.dll");
                }
            }
            catch(JsonException e)
            {
                throw new FormatException("meta_info.jsonファイルを読み込めません。", e);
            }
            DeviceName ??= string.Empty;
            DeviceID ??= string.Empty;
        }
        public bool IsCorrectFormat
        {
            get
            {
                return DeviceName.Length != 0 && DeviceID.Length != 0
                    && MetaInfoFile.Exists && MetaInfoFile.Extension == ".json"
                    && ConfigFile.Exists && ConfigFile.Extension == ".json"
                    && DeviceAssemblyFile.Exists && DeviceAssemblyFile.Extension == ".dll";
            }
        }
        public static DirectoryInfo? GetDeviceRootFolder()
        {
            var exeLocation = Assembly.GetEntryAssembly()?.Location;
            if (exeLocation is null) return null;
            var exe = new FileInfo(exeLocation);
            var exeDirectory = exe.Directory;
            if (exeDirectory is null) return null;
            var path = exeDirectory.FullName + "/Devices";
            var directory = new DirectoryInfo(exeLocation);
            if (directory.Exists) { return directory; }
            else return Directory.CreateDirectory(path);
        }
        public sealed class Generator : IDisposable
        {
            IODevice Device { get; }
            public DirectoryInfo DeviceDirectory { get; }
            public DeviceFolder DeviceFolder { get; }
            FileInfo AssemblyFile { get; }
            /// <summary>
            /// 既存のマシンフォルダからジェネレータを生成します。
            /// </summary>
            /// <param name="deviceDirectory">対象のマシンフォルダ</param>
            public Generator(DirectoryInfo deviceDirectory) : this(new DeviceFolder(deviceDirectory)) { }
            public Generator(DeviceFolder folder)
            {
                DeviceDirectory = folder.DeviceDirectory;
                AssemblyFile = folder.DeviceAssemblyFile;
                var deviceAssemblyFile = new FileInfo($"{folder.DeviceDirectory.FullName}/{DeviceDirectory.Name}.dll");
                Device = MachineLoader.GetDevice(deviceAssemblyFile, null);
                DeviceFolder = new DeviceFolder(DeviceDirectory);
            }
            /// <summary>
            /// Dllファイルから新規にジェネレータを生成します。
            /// </summary>
            /// <param name="assemblyFile">対象のマシンDLLファイル</param>
            public Generator(FileInfo assemblyFile)
            {
                AssemblyFile = assemblyFile;
                Device = MachineLoader.GetDevice(assemblyFile, null);
                var root = GetDeviceRootFolder();
                if (root is null)
                {
                    throw new IOException("マシンフォルダの取得に失敗しました。");
                }
                var dllName = assemblyFile.Name[..^assemblyFile.Extension.Length];
                var machineDirectoryPath = root.FullName + "/" + dllName;
                DeviceDirectory = new DirectoryInfo(machineDirectoryPath);
                DeviceDirectory.Create();
                File.Copy(assemblyFile.FullName, DeviceDirectory.FullName + "/" + assemblyFile.Name, true);
                DeviceFolder = new DeviceFolder(DeviceDirectory);
            }
            public Generator GenerateAll()
            {
                return GenerateConfig().GenerateMetaInfo();
            }
            public Generator GenerateMetaInfo()
            {
                if (disposed) return this;
                using (var metainfo = new Utf8JsonWriter(DeviceFolder.MetaInfoFile.Create()))
                {
                    var json = new JsonObject
                    {
                        new KeyValuePair<string, JsonNode?>(
                            "name", JsonValue.Create(Device.Name)
                        ),
                        new KeyValuePair<string, JsonNode?>(
                            "id", JsonValue.Create(Device.ID)
                        ),
                        new KeyValuePair<string, JsonNode?>(
                            "assembly",JsonValue.Create(AssemblyFile.Name)
                        )
                    };
                    json.WriteTo(metainfo);
                };
                return this;
            }
            public Generator GenerateConfig()
            {
                if (disposed) return this;
                using (var config = new Utf8JsonWriter(DeviceFolder.ConfigFile.Create()))
                {
                    Device.DeviceConfig.WriteRegistriesTo(config);
                }
                return this;
            }
            bool disposed = false;
            public void Dispose()
            {
                if (disposed) return;
                Device?.Dispose();
                disposed = true;
                GC.SuppressFinalize(this);
            }
            ~Generator()
            {
                Dispose();
            }
            
        }
    }
}
