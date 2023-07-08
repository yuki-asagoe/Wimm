﻿using System.Windows.Interop;
using Wimm.Machines.Tpip3.Import;

namespace Wimm.Machines.Tpip3
{
    /// <summary>
    /// Tpip3ボードで制御するロボットを表現するクラスです。
    /// </summary>
    /// <remarks>派生クラスはpublic (string,HwndSource)コンストラクタを提供する必要があります</remarks>
    public abstract class Tpip3Machine : Machine
    {
        /// <summary>
        /// Tpipの初期化が行われたか、falseの場合実際の制御処理を行う必要はありません。
        /// </summary>
        protected bool TpipInitialized { get; private init; }
        public const string ControlBoardName = "Tpip3";
        public override string ControlBoard => ControlBoardName;
        public override ConnectionState ConnectionStatus {
            get {
                return TPJT3.NativeMethods.get_com_mode() switch
                {
                    0=>ConnectionState.Offline,
                    1=>ConnectionState.ConnectionReady,
                    2 or 3=>ConnectionState.Connecting,
                    4=>ConnectionState.Online,
                    _=>ConnectionState.Unknown
                };
            } 
        }
        protected HwndSource? Hwnd { get; init; }
        protected string MachineIPAddress { get; init; }=string.Empty;
        protected Tpip3Machine(MachineConstructorArgs args):base(args)
        {
            Hwnd = HwndSource.FromHwnd(args.HostingWindowHandle);
            MachineConfig.AddRegistries();
            TpipInitialized = true;
            AddRegistries();
            if(MachineConfig.GetValueOrDefault("TPIP3_IP_Address") is string ipAddress)
            {
                args.Logger.Info($"接続先IPアドレス:{ipAddress}");
                MachineIPAddress = ipAddress;
            }
            TPJT3.NativeMethods.init(MachineIPAddress, Hwnd.Handle);
        }
        protected Tpip3Machine()
        {
            TpipInitialized = false;
            AddRegistries();
        }
        public override void Dispose()
        {
            GC.SuppressFinalize(this);
            if (TpipInitialized)
            {
                TPJT3.NativeMethods.close();
            }
            Hwnd?.Dispose();
        }
        private void AddRegistries()
        {
            MachineConfig.AddRegistries(
                new ConfigItemRegistry("TPIP3_IP_Address","192.168.0.200")
            );
        }
    }
}