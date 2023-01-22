using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Wimm.Machines.Audio;
using Wimm.Machines.Video;

namespace Wimm.Model.Video
{
    /// <summary>
    /// カメラ更新の処理とQRコードなどの検知を管理するクラス
    /// </summary>
    public class VideoProcessor:ICameraUpdateListener
    {
        public VideoProcessor(System.Drawing.Size imageSize,Camera camera)
        {
            ImageSize = imageSize;
            Camera = camera;
            camera.AddListener(this);
        }
        public System.Drawing.Size ImageSize { get; set; }
        Camera Camera { get; init; }
        /// <summary>
        /// 画像が更新された時に呼び出されます。
        /// 動作スレッドを保障しません、Dispatcherなどを用いて明示的にUIスレッドに結果をフィードバックすることを推奨します。
        /// </summary>
        public event ImageUpdateHandler? ImageUpdated;
        public delegate void ImageUpdateHandler(BitmapSource image);
        /// <summary>
        /// 画像加工処理を行います、非常に高頻度で呼び出されることが想定されるのであまり重たくしすぎないでください
        /// 重くするとコマ落ちの恐れがあります。
        /// </summary>
        private BitmapSource Draw(FrameData[] frames)
        {
            return frames[0].Frame.ToBitmapSource();
        }
        public bool IsReadyToReceive => true;
        public void OnReceiveData(FrameData[] frames)
        {
            ImageUpdated?.Invoke(Draw(frames));
        }
    }
}
