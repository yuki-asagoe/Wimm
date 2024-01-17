using System.Runtime.InteropServices;
using System.Windows.Interop;
using Wimm.Common.Setting;
using Wimm.Machines.Extension;
using Wimm.Machines.TpipForRasberryPi.Import;

namespace Wimm.Machines.TpipForRasberryPi
{
    /// <summary>
    /// Tpip4ボードで制御するロボットを表現するクラスです。
    /// </summary>
    /// <remarks>派生クラスはpublic (MachineConstructorArgs?)コンストラクタを提供する必要があります</remarks>
    public abstract class TpipForRasberryPiMachine : Machine , IPowerVoltageProvidable
    {
        /// <summary>
        /// Tpipの初期化が行われたか、falseの場合実際の制御処理を行う必要はありません。
        /// </summary>
        protected bool TpipInitialized { get; private init; }
        public const string ControlBoardName = "Tpip4RP";
        public override string ControlSystem => ControlBoardName;
        public override ConnectionState ConnectionStatus
        {
            get
            {
                return TPJT4.NativeMethods.get_com_mode() switch
                {
                    0 => ConnectionState.Offline,
                    1 => ConnectionState.ConnectionReady,
                    2 or 3 => ConnectionState.Connecting,
                    4 => ConnectionState.Online,
                    _ => ConnectionState.Unknown
                };
            }
        }
        public IPowerVoltageProvidable.VoltageInfo[] Voltages
        {
            get
            {
                var data = new TPJT4.INP_DT_STR[1];
                TPJT4.NativeMethods.get_sens(data, Marshal.SizeOf<TPJT4.INP_DT_STR>());
                return new[] { new IPowerVoltageProvidable.VoltageInfo("Control",data[0].batt / 1000.0, 0, 30) };
            }
        }
        protected HwndSource? Hwnd { get; init; }
        protected string MachineIPAddress { get; init; } = string.Empty;
        protected TpipForRasberryPiMachine(MachineConstructorArgs? args):base(args)
        {
            TpipInitialized = false;
            AddRegistries();
            if(args is not null)
            {
                Hwnd = HwndSource.FromHwnd(args.HostingWindowHandle);
                if (MachineConfig.GetValueOrDefault("TPIP4RP_IP_Address") is string ipAddress)
                {
                    args.Logger.Info($"接続先IPアドレス:{ipAddress}");
                    MachineIPAddress = ipAddress;
                }
                TPJT4.NativeMethods.init(MachineIPAddress, Hwnd.Handle);
                TpipInitialized = true;
            }
        }
        public override void Dispose()
        {
            GC.SuppressFinalize(this);
            if (TpipInitialized)
            {
                TPJT4.NativeMethods.close();
            }
            Hwnd?.Dispose();
        }
        private void AddRegistries()
        {
            MachineConfig.AddRegistries(
                new ConfigItemRegistry("TPIP4RP_IP_Address", "192.168.0.200")
            );
        }
    }
}