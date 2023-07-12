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
    internal class BinarizationFilter : Filter
    {
        public override ImmutableArray<DoubleParameter> DoubleParameters { get; } = new[]
        {
            new DoubleParameter("閾値",0,1)
        }.ToImmutableArray();

        public override ImmutableArray<BooleanParameter> BooleanParameters { get; } =  new[]
        {
            new BooleanParameter("大津の二値化")
        }.ToImmutableArray();

        public override PackIconModernKind Icon => PackIconModernKind.TypeBit;

        public override string Name => "二値化";

        public override void Apply(Mat frame)
        {
            Cv2.CvtColor(frame, frame, ColorConversionCodes.BGR2GRAY);
            if (BooleanParameters[0].Value)//大津の二値化
            {
                Cv2.Threshold(frame, frame, 255 * DoubleParameters[0].Value, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
            }
            else
            {
                Cv2.Threshold(frame, frame, 255 * DoubleParameters[0].Value, 255, ThresholdTypes.Binary);
            }
        }
    }
}
