using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Machines.Video
{
    internal class DummyCamera : Camera
    {
        public override bool SupportingMultiObservation => false;
        public DummyCamera()
        {
            Channels = ImmutableArray<Channel>.Empty;
        }
    }
}
