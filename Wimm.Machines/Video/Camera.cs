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
                foreach(var c in Channels) { if(c != channel)c.IsActive = false; }
            }
            channel.IsActive = true;
        }
        public void Activate(int index,bool activation)
        {
            if (0 <= index && index < Channels.Length)
            {
                Activate(Channels[index],activation);
            }
        }
        public ImmutableArray<Channel> ActiveChannels { get { return Channels.Where(it => it.IsActive).ToImmutableArray(); } }
        /// <summary>
        /// カメラの更新を受けるハンドラ
        /// </summary>
        /// <param name="sentFrames">送信されたカメラと画像のペア、画像は処理後にリソース解放が行われるので単に参照を保持し続けることは推奨しません。</param>
        public delegate void CameraUpdateHandler((Channel channel,Mat frame)[]sentFrames);
        public event CameraUpdateHandler? OnUpdate;
        /// <summary>
        /// カメラの更新通知を呼び出します
        /// </summary>
        /// <param name="frames">送信される画像とチャネルのペア</param>
        /// <param name="disposeAll">framesに渡された画像を処理後に解放するか、falseを渡す場合は解放処理を実装することを推奨します。</param>
        protected void CallUpdateHandler((Channel channel, Mat frame)[] frames,bool disposeAll=true) {
            if(OnUpdate is not null)OnUpdate (frames);
            if(disposeAll)foreach (var i in frames)
            {
                if (!i.frame.IsDisposed) i.frame.Dispose();
            }
        }
    }
    public class Channel
    {
        protected Channel(string name) { Name = name; }
        public string Name { get; private init; }
        private bool active;
        public event Action<bool>? ActivationChanged;
        public bool IsActive { get { return active; } internal set { active = value; ActivationChanged?.Invoke(value); } }
    }
}