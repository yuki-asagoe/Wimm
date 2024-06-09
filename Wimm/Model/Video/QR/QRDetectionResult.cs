using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ZXing;

namespace Wimm.Model.Video.QR
{
    internal class QRDetectionResult
    {
        public string Text { get; }
        public string RawBytesAsString { get; }
        public byte[] RawBytes { get; }
        public BitmapSource? QRImage { get; }

        public QRDetectionResult(Result result, Mat image)
        {
            Text = result.Text;
            RawBytes = result.RawBytes[0..(result.NumBits / 8)];
            RawBytesAsString = BitConverter.ToString(RawBytes);
            try
            {
                int minX = (int)Math.Clamp(result.ResultPoints.Select(p => p.X).Min(), 0, image.Cols);
                int minY = (int)Math.Clamp(result.ResultPoints.Select(p => p.Y).Min(), 0, image.Rows);
                int width = (int)Math.Clamp(result.ResultPoints.Select(p => p.X).Max() - minX, 0, image.Cols);
                int height = (int)Math.Clamp(result.ResultPoints.Select(p => p.Y).Max() - minY, 0, image.Rows);
                using var clippedImage = image.Clone(new OpenCvSharp.Rect(
                       minX, minY, width, height
                ));
                var clippedImageSource = clippedImage.ToWriteableBitmap();
                clippedImageSource.Freeze();
                QRImage = clippedImageSource;
            }
            catch(OpenCVException _)
            {
                QRImage = null;
            }
        }
    }
}
