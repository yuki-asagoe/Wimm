using Wimm.Machines.Component;
using Wimm.Machines.Video;
using System.Collections.Immutable;
using System.Text.Json;

namespace Wimm.Machines
{
    /// <summary>
    /// ロボットを表現するクラス
    /// </summary>
    /// <remarks>
    /// MachineConstructorArgsを受け取るコンストラクタと、引数無しのコンストラクタを提供する必要があります。
    /// 特別のコンストラクタを提供するサブクラスを継承した上で、無引数コンストラクタが実際の操作に不十分であっても構いません。(ドキュメント生成用だから
    /// </remarks>
    public abstract class Machine : IDisposable
    {
        public Machine(MachineConstructorArgs args)
        {
            var configFileName = args.MachineDirectory.FullName + "/config.json";
            if (File.Exists(configFileName))
            {
                var bytes = File.ReadAllBytes(configFileName);
                MachineConfig = new MachineConfig(new Utf8JsonReader(new ReadOnlySpan<byte>(bytes)));
            }
            else
            {
                MachineConfig = new MachineConfig();
            }
        }
        public Machine()
        {
            MachineConfig = new MachineConfig();
        }
        private double speedModifier = 1;
        /// <summary>
        /// モーターの回転などに適応する速度調整係数
        /// </summary>
        public double SpeedModifier
        {
            get { return speedModifier; }
            set { speedModifier = Math.Clamp(value, 0, 1); }
        }
        public abstract string Name { get; }
        public abstract string ControlBoard { get; }
        public abstract ConnectionState ConnectionStatus { get; }
        public abstract Camera Camera { get; }
        public MachineConfig MachineConfig { get; }
        /// <summary>
        /// 構造化された、つまり意味のある単位にグループ化されたModuleGroup
        /// </summary>
        public ModuleGroup StructuredModules { get; protected init; }
            = new ModuleGroup("",ImmutableArray<ModuleGroup>.Empty, ImmutableArray<Module>.Empty);
        public virtual void Reset() {}
        public virtual void Dispose(){ }
        private MachineState state=MachineState.Idle;
        public MachineState Status
        {
            get { return state; }
            set {
                state = value;
                OnStatusChanged(value);
            }
        }
        protected virtual void OnStatusChanged(MachineState newState) {}
        public ControlProcess? StartControl()
        {
            if(Status is MachineState.Active)
            {
                return StartControlProcess();
            }
            return null;
        }
        /// <summary>
        /// 制御処理を開始する際に呼び出されます。
        /// </summary>
        /// <returns>その処理単位を表現するControlProcess</returns>
        protected virtual ControlProcess StartControlProcess() => new ControlProcess();
    }
    public enum ConnectionState
    {
        Offline,Online,Connecting,ConnectionReady,Unknown
    }
    /// <summary>
    /// 制御処理を表現するクラスです。
    /// </summary>
    /// <remarks>
    /// 制御信号の送信にあたって情報をバッファし最後にまとめて送信する必要がある場合などに利用できます。
    /// </remarks>
    public class ControlProcess : IDisposable
    {
        /// <summary>
        /// 制御処理が終了する際に呼び出されます。
        /// </summary>
        public virtual void Dispose() { }
    }
}