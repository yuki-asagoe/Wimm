using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Machines.Audio
{
    public abstract class Channel
    {
        public string Name { get; }
        /// <summary>
        /// 1サンプル当たりのビット数
        /// </summary>
        public int BitsPerSample { get; }
        /// <summary>
        /// 1秒あたりのサンプル数
        /// </summary>
        public int SamplingRate { get; }

        public event EventHandler<AudioInputEventArgs>? OnDataAvailable;

        public Channel(string name,int bitPerSample,int samplingRate)
        {
            Name = name;
            BitsPerSample = bitPerSample;
            SamplingRate = samplingRate;
        }
        internal abstract Span<byte> GetData();
    }
}
