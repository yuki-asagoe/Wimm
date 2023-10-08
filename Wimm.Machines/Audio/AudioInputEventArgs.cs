using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Machines.Audio
{
    public class AudioInputEventArgs : EventArgs
    {
        Channel Channel { get; }
        /// <summary>
        /// データへの参照を返します。
        /// 変数へのキャプチャを推奨します。
        /// </summary>
        public Span<byte> Data
        {
            get
            {
                return Channel.GetData();
            }
        }
        public AudioInputEventArgs(Channel channel)
        {
            Channel = channel;
        }
    }
}
