using System.Windows.Interop;
using Wimm.Machines.TpipForRasberryPi.Import;

namespace Wimm.Machines.TpipForRasberryPi
{
    /// <summary>
    /// Tpip4ボードで制御するロボットを表現するクラスです。
    /// </summary>
    /// <remarks>派生クラスはpublic (string,HwndSource)コンストラクタを提供する必要があります</remarks>
    public abstract class TpipForRasberryPiMachine : Machine
    {
        /// <summary>
        /// Tpipの初期化が行われたか、falseの場合実際の制御処理を行う必要はありません。
        /// </summary>
        protected bool TpipInitialized { get; private init; }
        public const string ControlBoardName = "Tpip4RP";
        public override string ControlBoard => ControlBoardName;
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
        protected HwndSource? Hwnd { get; init; }
        protected TpipForRasberryPiMachine(string machineIpAddress, HwndSource hwnd)
        {
            Hwnd = hwnd;
            TpipInitialized = true;
            TPJT4.NativeMethods.init(machineIpAddress, hwnd.Handle);
        }
        protected TpipForRasberryPiMachine()
        {
            TpipInitialized = false;
        }
    }
}