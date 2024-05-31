using System.Runtime.InteropServices;
using System.Windows.Interop;
using Wimm.Common.Setting;
using Wimm.Machines.Extension;
using Wimm.Machines.Tpip3.Import;

namespace Wimm.Machines.Tpip3
{
    /// <summary>
    /// Tpip3ボードで制御するロボットを表現するクラスです。
    /// </summary>
    /// <remarks>派生クラスはpublic (MachineConstructorArgs?)コンストラクタを提供する必要があります</remarks>
    public abstract class Tpip3Machine : Machine,IPowerVoltageProvidable
    {
        /// <summary>
        /// Tpipの初期化が行われたか、falseの場合実際の制御処理を行う必要はありません。
        /// </summary>
        protected bool TpipInitialized { get; private init; }
        public const string ControlBoardName = "Tpip3";
        public override string ControlSystem => ControlBoardName;
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
        public IPowerVoltageProvidable.VoltageInfo[] Voltages {
            get
            {
                var data = new TPJT3.INP_DT_STR[1];
                TPJT3.NativeMethods.get_sens(data, Marshal.SizeOf<TPJT3.INP_DT_STR>());
                return new[] { new IPowerVoltageProvidable.VoltageInfo("Control",data[0].batt / 100.0, 0, 30) };
            }
        }

        protected Tpip3Machine(MachineConstructorArgs? args):base(args)
        {
            
            MachineConfig.AddRegistries();
            TpipInitialized = false;
            AddRegistries();
            if(args is not null)
            {
                Hwnd = HwndSource.FromHwnd(args.HostingWindowHandle);
                if (MachineConfig.GetValueOrDefault("TPIP3_IP_Address") is string ipAddress)
                {
                    args.Logger.Info($"接続先IPアドレス:{ipAddress}");
                    MachineIPAddress = ipAddress;
                }
                TPJT3.NativeMethods.init(MachineIPAddress, Hwnd.Handle);
                TpipInitialized = false;
            }
            
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