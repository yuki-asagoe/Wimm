using MahApps.Metro.IconPacks;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Model.Video
{
    public abstract class Filter
    {
        public abstract void Apply(Mat frame);
        public abstract ImmutableArray<DoubleParameter> DoubleParameters { get; }
        public abstract ImmutableArray<BooleanParameter> BooleanParameters { get; }
        public abstract PackIconModernKind Icon { get; }
        public abstract String Name { get; }
    }
    public record DoubleParameter(string Name,double Min,double Max)
    {
        private double _value=Min;
        public double Value
        {
            get { return _value; }
            set { _value = Math.Clamp(value, Min, Max); }
        }
    }
    public record BooleanParameter(string Name,bool Value=false);
}
