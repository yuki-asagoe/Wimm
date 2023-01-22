using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Wimm.Machines.Tpip3.Import;
using Wimm.Machines.Video;

namespace Wimm.Machines.Tpip3
{
    public class Tpip3Camera : Camera
    {
        public Tpip3Camera(params string[] names)
        {
            if (names.Length > 4) { throw new ArgumentException("カメラは4つまでしか設定できません"); }
            Channels = names.Select(it => (Channel)new Tpip3CameraChannel(it)).ToImmutableArray();
        }
        public override bool SupportingMultiObservation => false;
        private const int WM_PAINT = 0x000F;
        private ulong lastTimeStamp = 0;
        private int activeChannelIndex = 0;
        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_PAINT)
            {
                if (!CanDataSend) { return IntPtr.Zero; }
                ulong timeStamp = TPJT3.NativeMethods.JF_get_stamp();
                if (lastTimeStamp < timeStamp)
                {
                    lastTimeStamp = timeStamp;
                    int byteSize = 0;
                    var arrayAddress = TPJT3.NativeMethods.get_jpeg_file(0, 0, ref byteSize);
                    if (arrayAddress != IntPtr.Zero && byteSize > 0)
                    {
                        byte[] data = new byte[byteSize];
                        Marshal.Copy(arrayAddress, data, 0, byteSize);
                        var image = Cv2.ImDecode(data, ImreadModes.Unchanged);
                        CallUpdateHandler(new[] { new FrameData(Channels[activeChannelIndex], image) });
                        handled = true;
                    }
                    TPJT3.NativeMethods.free_jpeg_file();
                }
            }
            return IntPtr.Zero;
        }
        public override void Activate(Channel channel, bool activation)
        {
            base.Activate(channel, activation);
            activeChannelIndex = Channels.IndexOf(channel);
            if(activation)TPJT3.NativeMethods.chg_camera(activeChannelIndex);
        }
    }
    public class Tpip3CameraChannel : Channel
    {
        public Tpip3CameraChannel(string name):base(name){}
    }
}
