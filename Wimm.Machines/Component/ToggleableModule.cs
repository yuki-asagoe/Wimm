using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Machines.Component
{
    /// <summary>
    /// 切り替え可能なモジュールを提供するクラスです。
    /// </summary>
    public abstract class ToggleableModule : Module
    {
        protected ToggleableModule(string name, string description) : base(name, description)
        {
            var builder = ImmutableArray.Create(SwitchFeature.CastToBaseForm());
            Features = builder.ToImmutableArray();
        }
        /// <summary>
        /// 切り替えを表現するFeatureとして推奨される名前
        /// </summary>
        /// <remarks>明確な意図が無い場合はこの名前を使用してください</remarks>
        public const string SwitchFeatureName = "switch";
        /// <summary>
        /// 切り替えを表現するFeatureを返します。
        /// </summary>
        public abstract Feature<Action<bool>> SwitchFeature { get; }
        public void Switch(bool status) => SwitchFeature.Function(status);
    }
}
