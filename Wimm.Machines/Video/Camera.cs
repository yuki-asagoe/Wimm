using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace Wimm.Machines.Video
{
    public abstract class Camera
    {
        /// <summary>
        /// 複数のカメラを同時に扱うことができるならtrue、でなければfalse
        /// </summary>
        public abstract bool SupportingMultiObservation { get; }
        public ImmutableArray<Channel> Channels { get; protected init; }
        public virtual void Activate(Channel channel,bool activation)
        {
            if (!activation)
            {
                channel.IsActive = false;
                return;
            }
            if (!SupportingMultiObservation)
            {
                foreach(var c in Channels) { c.IsActive = false; }
            }
            channel.IsActive = true;
        }
        public ImmutableArray<Channel> ActiveChannels { get { return Channels.Where(it => it.IsActive).ToImmutableArray(); } }
        /// <summary>
        /// 現在アクティブになっているカメラの画像
        /// </summary>
        public ImmutableArray<Mat> ActiveFrames => Channels
                    .Where(it => it.IsActive)
                    .Select(it => it.RawFrame)
                    .Where(it => it is not null)
                    .ToImmutableArray();
        public delegate void CameraUpdateHandler(Channel[] updatedChannels);
        public event CameraUpdateHandler? OnUpdate;
        protected void CallUpdateHandler(Channel[] updated) { if(OnUpdate is not null)OnUpdate (updated); }
    }
    public class Channel
    {
        protected Channel(string name) { Name = name; }
        public string Name { get; private init; }
        public bool IsActive { get; internal set; }
        public Mat? RawFrame { get; protected set; }
        /// <summary>
        /// 現在の画像を複製したものを返します。
        /// 画像にフィルタなどを適用する場合には元画像が書き変わることを防ぐためにこちらを利用してください。
        /// この関数が返す値はディープコピーなので極端に高頻度で呼び出すとパフォーマンス上の問題を引き起こす恐れがあります。
        /// </summary>
        /// <returns>画像のディープコピー。存在しない場合はnull</returns>
        public Mat? CloneFrame() => RawFrame?.Clone();
    }
}