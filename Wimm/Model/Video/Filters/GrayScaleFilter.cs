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
    internal class GrayScaleFilter : Filter
    {
        public override ImmutableArray<DoubleParameter> DoubleParameters => ImmutableArray<DoubleParameter>.Empty;

        public override ImmutableArray<BooleanParameter> BooleanParameters => ImmutableArray<BooleanParameter>.Empty;

        public override PackIconModernKind Icon => PackIconModernKind.Opacity;

        public override string Name => "グレースケール";

        public override void Apply(Mat frame)
        {
            Cv2.CvtColor(frame, frame, ColorConversionCodes.BGR2GRAY);
        }
    }
}
