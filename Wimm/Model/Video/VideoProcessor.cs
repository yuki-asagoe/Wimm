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
            camera.SetListener(this);
        }
        public System.Drawing.Size ImageSize { get; set; }
        Camera Camera { get; init; }
        /// <summary>
        /// 画像が更新された時に呼び出されます。
        /// 動作スレッドを保障しません、Dispatcherなどを用いて明示的にUIスレッドに結果をフィードバックすることを推奨します。
        /// </summary>
        public event ImageUpdateHandler? ImageUpdated;
        /// <summary>
        /// QRコードのデータが更新された時に呼び出されます。
        /// 動作スレッドを保障しません、Dispatcherなどを用いて明示的にUIスレッドに結果をフィードバックすることを推奨します。
        /// </summary>
        public event QRResultUpdateHandler? QRUpdated;
        public delegate void QRResultUpdateHandler(QRDetectionResult result);
        public delegate void ImageUpdateHandler(BitmapSource image);
        /// <summary>
        /// 画像加工処理を行います、非常に高頻度で呼び出されることが想定されるのであまり重たくしすぎないでください
        /// 重くするとコマ落ちの恐れがあります。
        /// </summary>
        private BitmapSource Draw(FrameData[] frames)
        {
            var image = frames[0].Frame.ToBitmapSource();
            image.Freeze();
            return image;
        }
        public bool IsReadyToReceive => true;
        public async void OnReceiveData(FrameData[] frames)
        {
            if(qrDetectionTask?.IsCompleted??true && QRDetecting)
            {
                qrDetectionTask = DetectQR(frames[0].Frame.Clone());
                ImageUpdated?.Invoke(Draw(frames));
                var result=await qrDetectionTask;
                if(result is not null)
                {
                    QRDetecting = false;
                    QRUpdated?.Invoke(result);
                }
            }
            else
            {
                ImageUpdated?.Invoke(Draw(frames));
            }
        }
        private Task<QRDetectionResult?> DetectQR(Mat image)
        {
            return Task.Run(() =>
            {
                using var target=image;
                using var detector = new QRCodeDetector();
                var result=detector.DetectAndDecode(image, out var area);
                if(result is null || result.Length == 0 || area is null)
                {
                    return null;
                }
                using var clippedImage=image.Clone(new OpenCvSharp.Rect((int)area[0].X, (int)area[0].Y, (int)(area[1].X - area[0].X), (int)(area[1].Y - area[0].Y)));
                if(clippedImage is null)
                {
                    return null;
                }
                var clippedImageSource = clippedImage.ToWriteableBitmap();
                clippedImageSource.Freeze();
                return new QRDetectionResult(result, clippedImageSource);
            });
        }
        private Task<QRDetectionResult?>? qrDetectionTask = null;
        public bool QRDetecting { get; set; }
    }
    public record QRDetectionResult(string Result,BitmapSource? DetectedArea);
}
