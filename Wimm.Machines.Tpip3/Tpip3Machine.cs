using System.Windows.Interop;
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
        private HwndSource? Hwnd { get; init; }
        protected Tpip3Machine(string machineIpAddress,HwndSource hwnd)
        {
            Hwnd = hwnd;
            TpipInitialized = true;
            if (Camera is Tpip3Camera camera)
            {
                hwnd.AddHook(camera.WndProc);
            }
            TPJT3.NativeMethods.init(machineIpAddress, hwnd.Handle);
        }
        protected Tpip3Machine()
        {
            TpipInitialized = false;
        }
        public override void Dispose()
        {
            TPJT3.NativeMethods.close();
        }
    }
}