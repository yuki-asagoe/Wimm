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
    internal class ReverseFilter : Filter
    {
        public override ImmutableArray<DoubleParameter> DoubleParameters => Array.Empty<DoubleParameter>().ToImmutableArray();

        public override ImmutableArray<BooleanParameter> BooleanParameters => new BooleanParameter[]{
            new BooleanParameter("左右反転",true),
            new BooleanParameter("上下反転")
        }.ToImmutableArray();

        public override PackIconModernKind Icon => PackIconModernKind.ArrowLeftRight;

        public override string Name => "反転";

        public override void Apply(Mat frame)
        {
            FlipMode mode = ((BooleanParameters[0].Value ? 0b10 : 0) | (BooleanParameters[1].Value ? 1 : 0)) switch
            {
                0b11 => FlipMode.XY,
                0b10 => FlipMode.X,
                0b01 => FlipMode.Y,
                _ => (FlipMode) byte.MaxValue,
            };
            if ((byte)mode == byte.MaxValue) return;
            Cv2.Flip(frame, frame, mode);
        }
    }
}
