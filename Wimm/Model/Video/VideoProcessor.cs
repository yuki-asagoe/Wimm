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
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Wimm.Machines.Audio;
using Wimm.Machines.Video;

namespace Wimm.Model.Video
{
    public class VideoProcessor
    {
        public VideoProcessor(System.Drawing.Size imageSize,Camera camera,Dispatcher dispatcher)
        {
            ImageSize = imageSize;
            Camera = camera;
            UiDispatcher = dispatcher;
        }
        public System.Drawing.Size ImageSize { get; set; }
        Dispatcher UiDispatcher { get; init; }
        Camera Camera { get; init; }
        public event ImageUpdateHandler? OnImageUpdated;
        public delegate void ImageUpdateHandler(BitmapSource image);
        public async void OnUpdateImage(Channel[] channels)
        {
            var frameSize = ImageSize; // Drawメソッド内は別スレッドなので排他制御の代わりに外部で取得
            var image = await Task.Run(() => { return Draw(Camera, channels,frameSize); });
            UiDispatcher.Invoke(()=> { if (OnImageUpdated is not null) OnImageUpdated(image); });
        }
        private BitmapSource Draw(Camera camera, Channel[] updated, System.Drawing.Size frameSize)
        {
            //TODO さしあたって単一画像にしか対応してないので複数カメラへの対応ガンバ
            var frame = new Mat(ImageSize.Width, ImageSize.Height, MatType.CV_16SC3);
            Mat? image = updated[0].RawFrame;//色空間変換いる? 排他制御いる?
            if (image is null) return frame.ToBitmapSource();
            double aspectFrame = frame.Width / frame.Height;
            double aspectImage = image.Width / image.Height;
            Mat resized;
            if (aspectFrame > aspectImage)//フレームの方が横長
            {
                resized=image.Resize(new OpenCvSharp.Size(frame.Height * aspectImage, frame.Height));
            }
            else//フレームの方が縦長
            {
                resized = image.Resize(new OpenCvSharp.Size(frame.Width, frame.Width / aspectImage));
            }
            frame[new OpenCvSharp.Rect(new OpenCvSharp.Point(0, 0), resized.Size())]=resized;
            var result =frame.ToBitmapSource();
            result.Freeze();
            return result;
        }
    }
}
