using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Wimm.Machines.Video;
using Wimm.Model.Video.QR;

namespace Wimm.Model.Video
{
    /// <summary>
    /// カメラ更新の処理とQRコードなどの検知を管理するクラス
    /// </summary>
    internal class VideoProcessor:ICameraUpdateListener
    {
        public VideoProcessor(System.Drawing.Size imageSize,Camera camera)
        {
            ImageSize = imageSize;
            Camera = camera;
            camera.SetListener(this);
            Detector.OnDetected += (it) => OnQRDetected?.Invoke(it);
        }
        private QRDetector Detector { get; } = new QRDetector();
        public System.Drawing.Size ImageSize { get; set; }
        //コレクションにして複数のフィルタを組み合わせられるようにしたかったが制御と利用が煩雑化することを懸念して断念する
        public Filter? VideoFilter { private get; set; } = null;
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
        public event QRDetector.DetectionResultNotificator? OnQRDetected;
        public delegate void ImageUpdateHandler(BitmapSource image);
        /// <summary>
        /// 画像加工処理を行います、非常に高頻度で呼び出されることが想定されるのであまり重たくしすぎないでください
        /// 重くするとコマ落ちの恐れがあります。
        /// </summary>
        private Task<BitmapSource> Draw(FrameData[] frames)
        {
            return Task.Run(() =>
            {
                var frame=frames[0].Frame;
                VideoFilter?.Apply(frame);
                var image = frame.ToBitmapSource();
                image.Freeze();
                return image;
            });
            
        }
        public bool IsReadyToReceive => DrawTask?.IsCompleted ?? true;
        public Task<BitmapSource>? DrawTask { get; set; }
        public void OnReceiveData(FrameData[] frames)
        {
            DrawTask = Draw(frames);
            //このメソッドがUIスレッドと非同期に実行されるので同期待機で問題ない
            //DrawTaskがTaskなのは IsReadyToReceive で完了状態を参照するため
            DrawTask.Wait();
            ImageUpdated?.Invoke(DrawTask.Result);
            if (IsQRDetectionRequested)
            {
                using Mat image = frames[0].Frame.Clone();
                Detector.RequestDetection(image);
            }
        }
        public bool IsQRDetectionRequested { get; set; } = false;
    }
}
