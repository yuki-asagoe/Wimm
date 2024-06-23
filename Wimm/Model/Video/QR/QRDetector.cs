using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using ZXing.Multi.QrCode;
using ZXing.OpenCV;

namespace Wimm.Model.Video.QR
{
    internal class QRDetector
    {
        private BarcodeReader CodeReader { get; } = new BarcodeReader();
        private bool QRDetectionAvailable
        {
            get => DetectionTask?.IsCompleted ?? true ;
        }
        public event DetectionResultNotificator? OnDetected;
        public delegate void DetectionResultNotificator(IEnumerable<QRDetectionResult> result);
        private Task<ImmutableArray<QRDetectionResult>>? DetectionTask { get; set; } = null;
        public async void RequestDetection(Mat image)
        {
            if (QRDetectionAvailable)
            {
                DetectionTask = Task.Run(() =>
                {
                    var result= CodeReader.DecodeMultiple(image);
                    return result.Select(it => new QRDetectionResult(it, image)).ToImmutableArray();
                });
                var result = await DetectionTask;
                OnDetected?.Invoke(result);
            }
        }
    }
}
