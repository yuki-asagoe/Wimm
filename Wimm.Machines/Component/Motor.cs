using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Machines.Component

{
    /// <summary>
    /// モーターを表現するモジュール
    /// </summary>
    /// <remarks>
    /// Featuresに自動的にRotationFeatureを登録します。機能追加が必要なら上書きして追加してください。
    /// </remarks>
    public abstract class Motor : Module
    {
        protected Motor(string name, string description) : base(name, description)
        {
            var builder = ImmutableArray.Create(RotationFeature.CastToBaseForm());
            Features = builder.ToImmutableArray();
        }
        /// <summary>
        /// 回転を表現するFeatureとして推奨される名前
        /// </summary>
        /// <remarks>明確な意図が無い場合はこの名前を使用してください</remarks>
        public static string RotationFeatureName = "rotate";
        /// <summary>
        /// モータの回転を表すFeatureを返します
        /// </summary>
        /// <remarks>
        /// double:引数は 1 ~ -1 の範囲でそれぞれモータの正回転と逆回転の量を表します。
        /// 1,-1の時それぞれ最高出力
        /// </remarks>
        public abstract Feature<Action<double>> RotationFeature { get; }
        /// <summary>
        /// Featureを用いてモータを回転させます
        /// </summary>
        /// <param name="speed">1 ~ -1の範囲でモータの正回転と逆回転の量</param>
        public void Rotate(double speed)=>RotationFeature.Function(speed);
    }
}
