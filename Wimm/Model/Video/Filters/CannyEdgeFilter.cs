using MahApps.Metro.IconPacks;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Model.Video.Filters
{
    internal class CannyEdgeFilter : Filter
    {
        public override ImmutableArray<DoubleParameter> DoubleParameters { get; } = new[]
        {
            new DoubleParameter("検出最大値",0,255),
            new DoubleParameter("検出最小値",0,255)
        }.ToImmutableArray();

        public override ImmutableArray<BooleanParameter> BooleanParameters { get; } = new[]
        {
            new BooleanParameter("自動閾値選択(実験的)")
        }.ToImmutableArray();

        public override PackIconModernKind Icon => PackIconModernKind.PageCornerGrid;

        public override string Name => "エッジ検出";

        public override void Apply(Mat frame)
        {
            double minValue,maxValue;
            if (BooleanParameters[0].Value)//自動閾値
            {
                //これでも多分画像の内部フォーマット次第では上手くいかないよね
                byte[] data = new byte[frame.Width * frame.Height];
                Marshal.Copy(frame.Data, data, 0, data.Length);
                long median = 0;
                for(int i = 0; i < frame.Height; i++)
                {
                    for (int j = 0; j < frame.Width; j++)
                    {
                        median += data[frame.Width * i + j];
                    }
                    median /= frame.Width;
                }
                const double Delta = 0.3;
                minValue = Math.Clamp((1 - Delta) * median, 0, 255);
                maxValue = Math.Clamp((1 + Delta) * median, 0, 255);
            }
            else
            {
                minValue = DoubleParameters[1].Value;
                maxValue = Math.Max(minValue, DoubleParameters[0].Value);
            }

            Cv2.CvtColor(frame, frame, ColorConversionCodes.BGR2GRAY);
            frame.ConvertTo(frame, MatType.CV_16SC3);
            Cv2.Canny(frame, frame, frame, minValue, maxValue);
        }
    }
}
