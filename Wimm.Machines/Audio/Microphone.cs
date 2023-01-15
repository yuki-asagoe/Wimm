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
    /// ロボットからの音声入力を表現するクラス
    /// </summary>
    public abstract class Microphone
    {
        public ImmutableArray<Channel> Channels { get; protected init; }
        public class Channel
        {
            public bool IsActive { get; internal set; }
            public virtual int SampleRate { get; set; }
            public int MaxBufferSize { get; protected init; } = 1024;
            public Queue<short> SoundData { get; set; } = new Queue<short>();
        }
    }
}
