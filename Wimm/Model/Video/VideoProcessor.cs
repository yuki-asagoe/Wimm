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
    public class VideoProcessor
    {
        public VideoProcessor(System.Drawing.Size imageSize,Camera camera)
        {
            ImageSize = imageSize;
            Camera = camera;
            camera.OnUpdate += OnUpdateImage;
        }
        public System.Drawing.Size ImageSize { get; set; }
        private Task<BitmapSource>? frameProcessTask = null;
        Camera Camera { get; init; }
        /// <summary>
        /// 画像が更新された時に呼び出されます。
        /// 動作スレッドを保障しません、Dispatcherなどを用いて明示的にUIスレッドに結果をフィードバックすることを推奨します。
        /// </summary>
        public event ImageUpdateHandler? ImageUpdated;
        public delegate void ImageUpdateHandler(BitmapSource image);
        private async void OnUpdateImage((Channel channel, Mat frame)[] frames)
        {
            if (frameProcessTask?.IsCompleted ?? true)//仕事してない
            {
                frameProcessTask = Draw(frames);
                ImageUpdated?.Invoke(await frameProcessTask);
            }
        }
        /// <summary>
        /// 画像加工処理を行います、非常に高頻度で呼び出されることが想定されるのであまり重たくしすぎないでください
        /// 重くするとコマ落ちします。
        /// </summary>
        private async Task<BitmapSource> Draw((Channel channel,Mat frame)[] frames)
        {
            return await Task.Run(//まだ一画像にしか対応してないんよごめんね
                ()=> {
                    var i = frames[0].frame.ToBitmapSource();
                    i.Freeze();
                    return i;
                }
            );
        }
    }
}
