using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wimm.Machines;

namespace Wimm.Machines.Audio
{
    /// <summary>
    /// ロボットへの音声出力を表現するクラス
    /// </summary>
    public abstract class Speaker
    {
        public ImmutableArray<Channel> Channels { get; protected init; }
        public abstract class Channel
        {
            public abstract void Sound(IEnumerable<short> soundData,int sampleRate);
        }
    }
}
