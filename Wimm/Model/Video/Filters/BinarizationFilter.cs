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
        public override ImmutableArray<DoubleParameter> DoubleParameters => new[]
        {
            new DoubleParameter("閾値",0,1)
        }.ToImmutableArray();

        public override ImmutableArray<BooleanParameter> BooleanParameters => ImmutableArray<BooleanParameter>.Empty;

        public override void Apply(Mat frame)
        {
            Cv2.CvtColor(frame, frame, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(frame, frame, 255 * DoubleParameters[0].Value, 255,ThresholdTypes.Binary);
        }
    }
}
