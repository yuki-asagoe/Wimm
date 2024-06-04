using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
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
        public event QRResultUpdateHandler? QRUpdated;
        public delegate void QRResultUpdateHandler(QRDetectionResult result);
        public delegate void ImageUpdateHandler(BitmapSource image);
        /// <summary>
        /// 画像加工処理を行います、非常に高頻度で呼び出されることが想定されるのであまり重たくしすぎないでください
        /// 重くするとコマ落ちの恐れがあります。
        /// </summary>
        private BitmapSource Draw(FrameData[] frames)
        {
            var frame=frames[0].Frame;
            VideoFilter?.Apply(frame);
            var image = frame.ToBitmapSource();
            image.Freeze();
            return image;
        }
        public bool IsReadyToReceive => true;
        public async void OnReceiveData(FrameData[] frames)
        {
            if((qrDetectionTask?.IsCompleted??true) && QRDetecting)
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
                var result = detector.DetectAndDecode(image, out var area);

                if (result is null || result.Length == 0 || area is null)
                {
                    return null;
                }
                WriteableBitmap clippedImageSource;
                try
                {
                    //ゴリ押しこそ正義
                    int minX = (int)Math.Clamp(area.Select(x => x.X).Min(),0,image.Cols);
                    int minY = (int)Math.Clamp(area.Select(x => x.Y).Min(), 0, image.Rows);
                    int width = (int)Math.Clamp(area.Select(x => x.X).Max()-minX, 0, image.Cols);
                    int height = (int)Math.Clamp(area.Select(x => x.Y).Max()-minY,0,image.Rows);
                    using var clippedImage = image.Clone(new OpenCvSharp.Rect(
                        minX, minY, width, height
                    ));
                    if (clippedImage is null)
                    {
                        return null;
                    }
                    clippedImageSource = clippedImage.ToWriteableBitmap();
                    clippedImageSource.Freeze();
                    
                }
                catch (OpenCVException _)
                {
                    return new QRDetectionResult(result,null);
                }
                var allOneByte = result.All(it => (0xFF00 & it) == 0);
                if (allOneByte && result.Any(it => (it & 0b10000000) != 0)) // maybe binary?
                {
                    var byteArray = result.Select(it => (byte)it).ToArray();
                    EncodingProvider provider = CodePagesEncodingProvider.Instance;
                    var builder = new StringBuilder();
                    builder
                        .Append(result)
                        .Append('\n')
                        .AppendLine("Warning : This may be incorrect text")
                        .Append("Shift-JIS: ")
                        .AppendLine(provider.GetEncoding("shift-jis")?.GetString(byteArray))
                        .Append("0x: ")
                        .AppendLine(BitConverter.ToString(byteArray));
                    result = builder.ToString();
                }
                return new QRDetectionResult(result, clippedImageSource);
            });
        }
        private Task<QRDetectionResult?>? qrDetectionTask = null;
        public bool QRDetecting { get; set; }
    }
    public record QRDetectionResult(string Result,BitmapSource? DetectedArea);
}
