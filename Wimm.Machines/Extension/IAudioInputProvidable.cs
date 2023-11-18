using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wimm.Machines.Audio;

namespace Wimm.Machines.Extension
{
    public interface IAudioInputProvidable : IMachineExtension
    {
        public ImmutableArray<Channel> Channels { get; protected init; }
        public void StartRecording();
        public void StopRecording();
    }
}
