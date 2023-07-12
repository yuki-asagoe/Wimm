using MahApps.Metro.IconPacks;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Model.Video.Filters
{
    internal class LinearBrightnessCorrectionFilter : Filter
    {
        public override ImmutableArray<DoubleParameter> DoubleParameters { get; } = new DoubleParameter[]
        {
            new DoubleParameter("比例定数",0.2,1.8),
            new DoubleParameter("加算値",-100,100),
            new DoubleParameter("補正基準値(自動補正時)",0.1,0.9){Value = 0.5},
        }.ToImmutableArray();

        public override ImmutableArray<BooleanParameter> BooleanParameters { get; } = new BooleanParameter[]
        {
            new BooleanParameter("自動補正",true)
        }.ToImmutableArray();

        public override PackIconModernKind Icon => PackIconModernKind.Lightbulb;

        public override string Name => "明度補正[線形]";

        public override void Apply(Mat frame)
        {
            if (BooleanParameters[0].Value)
            {
                double brightness;
                {
                    var sum = frame.Sum();
                    brightness = (sum.ToDouble() / (255.0 * frame.Cols * frame.Rows));
                }

                Cv2.ConvertScaleAbs(frame, frame, alpha: DoubleParameters[2].Value / brightness, 0);
            }
            else
            {
                Cv2.ConvertScaleAbs(frame, frame, DoubleParameters[0].Value, DoubleParameters[1].Value);
            }
        }
    }
}
