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
        private Task? imageUpdateTask = null;
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
        private LinkedList<ICameraUpdateListener> Listeners { get; } = new LinkedList<ICameraUpdateListener>();
        public void AddListener(ICameraUpdateListener listener)
        {
            if (!Listeners.Contains(listener))
            {
                Listeners.AddLast(listener);
            }
        }
        public void RemoveListener(ICameraUpdateListener listener)
        {
            Listeners.Remove(listener);
        }
        /// <summary>
        /// カメラの更新通知を非同期に呼び出します。
        /// 画像リソースは処理後自動で解放されます。
        /// </summary>
        /// <param name="frames">送信される画像とチャネルのペア</param>
        protected void CallUpdateHandler(FrameData[] frames) {
            imageUpdateTask=Task.Run(() =>
            {
                foreach (var i in ListenableHandlers)
                {
                    i.OnReceiveData(frames);
                }
                foreach (var i in frames)
                {
                    if (!i.Frame.IsDisposed) i.Frame.Dispose();
                }
            });
        }
        /// <summary>
        /// 現在データを転送できる状態にあるかを返します
        /// </summary>
        /// <value><c>false</c> : 画像を転送できない状態である。更新のあったフレームは破棄しても問題ありません。</value>
        protected bool CanDataSend
        {
            get { return Listeners.Any((it) => it.IsReadyToReceive) && (imageUpdateTask?.IsCompleted??true/*暇してる*/); }
        }
        protected IEnumerable<ICameraUpdateListener> ListenableHandlers
        {
            get { return Listeners.Where((it) => it.IsReadyToReceive); }
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
    public interface ICameraUpdateListener
    {
        /// <summary>
        /// 現在画像データを処理できる状態であるかを返します
        /// </summary>
        bool IsReadyToReceive { get; }
        /// <summary>
        /// データを受け取った際呼び出されます。動作スレッドは不定です。
        /// </summary>
        void OnReceiveData(FrameData[] frames);
    }
    /// <summary>
    /// チャネルと送信された画像を扱うレコード
    /// </summary>
    /// <param name="Channel">画像を提供したChannel</param>
    /// <param name="Frame">対応する画像。画像は処理後にリソース解放が行われるので単に参照を保持し続けることは推奨しません。</param>
    public record FrameData(Channel Channel, Mat Frame) { }
}