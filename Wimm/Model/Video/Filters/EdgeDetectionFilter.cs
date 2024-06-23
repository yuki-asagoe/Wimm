using MahApps.Metro.IconPacks;
using OpenCvSharp;
using System;
using System.Collections.Immutable;

namespace Wimm.Model.Video.Filters
{
    internal class EdgeDetectionFilter : Filter
    {
        public override ImmutableArray<DoubleParameter> DoubleParameters { get; } = Array.Empty<DoubleParameter>().ToImmutableArray();

        public override ImmutableArray<BooleanParameter> BooleanParameters { get; } = Array.Empty<BooleanParameter>().ToImmutableArray();

        public override PackIconModernKind Icon => PackIconModernKind.PageCornerGrid;

        public override string Name => "エッジ検出";

        public override void Apply(Mat frame)
        {
            Cv2.CvtColor(frame, frame, ColorConversionCodes.BGR2GRAY);
            frame.ConvertTo(frame, MatType.CV_16SC1);
            Cv2.Laplacian(frame, frame, MatType.CV_16SC1);
        }
    }
}
