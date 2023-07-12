using MahApps.Metro.IconPacks;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Model.Video.Filters
{
    internal class GammaBrightnessCorrectionFilter : Filter
    {
        public override ImmutableArray<DoubleParameter> DoubleParameters { get; } = new DoubleParameter[]
        {
            new DoubleParameter("ガンマ補正度",-2,2){Value = 0},
            new DoubleParameter("補正基準値(自動補正時)",0.1,0.9){Value = 0.5},
        }.ToImmutableArray();

        public override ImmutableArray<BooleanParameter> BooleanParameters { get; } = new BooleanParameter[]
        {
            new BooleanParameter("自動補正",true)
        }.ToImmutableArray();

        public override PackIconModernKind Icon => PackIconModernKind.LightbulbCoil;

        public override string Name => "明度補正[ガンマ補正]";

        private byte[] LUT { get; } = new byte[256];
        private double oldGamma = double.NaN;

        public override void Apply(Mat frame)
        {
            double gamma;
            if (BooleanParameters[0].Value)
            {
                double brightness;
                {
                    var sum = frame.Sum();
                    brightness = (sum.ToDouble() / (255.0 * frame.Cols * frame.Rows));
                }
                gamma = Math.Clamp(brightness / DoubleParameters[1].Value, 1 / 4.0, 4);
            }
            else
            {
                gamma = Math.Pow(2, DoubleParameters[0].Value);
            }
            if (oldGamma != gamma)
            {
                for (var i = 0; i < 256; i++)
                {
                    LUT[i] = (byte)(Math.Pow(i / 255.0, gamma) * 255.0);
                }
                oldGamma = gamma;
            }
            Cv2.LUT(frame, LUT, frame);
        }
    }
}
