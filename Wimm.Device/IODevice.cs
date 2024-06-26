﻿using System.Collections.Immutable;
using System.Text.Json;
using Wimm.Common;
using Wimm.Common.Setting;

namespace Wimm.Device
{
    /// <summary>
    /// Wimmの拡張入出力デバイスを定義する基底抽象クラスです。
    /// </summary>
    public abstract class IODevice : IDisposable
    {
        /// <summary>
        /// ユーザに提供するデバイス名を返します。
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// デバイスの簡易的な説明を返します。
        /// </summary>
        public abstract string Description { get; }
        /// <summary>
        /// プログラムからデバイスの識別に使用する名前を返します。他のデバイスと衝突しない名前を指定してください。
        /// </summary>
        public abstract string ID { get; }
        public ImmutableArray<Feature<Delegate>> Features { get; protected init; }
        public Config DeviceConfig { get; }
        public ImmutableArray<InformationNode> Information { get; protected init; } = ImmutableArray<InformationNode>.Empty;

        public IODevice(DeviceConstructorArgs? args)
        {
            if(args is not null)
            {
                string configFile = args.DeviceDirectory.FullName + "/config.json";
                if (File.Exists(configFile))
                {
                    var bytes = File.ReadAllBytes(configFile);
                    DeviceConfig = new Config(new Utf8JsonReader(new ReadOnlySpan<byte>(bytes)));
                }
            }

            DeviceConfig ??= new Config();
        }
        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}