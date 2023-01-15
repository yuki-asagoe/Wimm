using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Machines.Component
{
    public abstract class ServoMotor : Module
    {
        protected ServoMotor(string name, string description) : base(name, description)
        {
            Features = ImmutableArray.Create(
                RotationFeature.CastToBaseForm(),
                SetAngleFeature.CastToBaseForm()
            );
        }
        /// <summary>
        /// 回転を表現するFeatureとして推奨される名前
        /// </summary>
        /// <remarks>明確な意図が無い場合はこの名前を使用してください</remarks>
        public static string RotationFeatureName = "rotate";
        /// <summary>
        /// 角度指定付き回転を表現するFeatureとして推奨される名前
        /// </summary>
        /// <remarks>明確な意図が無い場合はこの名前を使用してください</remarks>
        public static string SetAngleFeatureName = "setangle";
        /// <summary>
        /// 角度の指定を表現するFeatureを返します。
        /// </summary>
        /// <remarks>
        /// Action について
        /// 第一引数:double 目標の角度 基準から 180 ~ -180(単位:°)の範囲
        /// 第二引数:double 速度 最大値を基準に 1~0
        /// </remarks>
        public abstract Feature<Action<double, double>> SetAngleFeature { get; }
        /// <summary>
        /// 回転を表現するFeatureInfoを返します。
        /// </summary>
        /// <remarks>
        /// Action について
        /// 第一引数: double 速度 最大値を基準に 1~-1
        /// </remarks>
        public abstract Feature<Action<double>> RotationFeature { get; }
        /// <summary>
        /// モータを回転させます。
        /// </summary>
        /// <param name="speed">回転速度 1 ~ -1</param>
        public void Rotate(double speed) => RotationFeature.Function(speed);
        /// <summary>
        /// モータを角度を指定して回転させます
        /// </summary>
        /// <param name="angle">角度 基準から 180 ~ -180</param>
        /// <param name="speed">速度 最高速度基準に 1~0</param>
        public void SetAngle(double angle,double speed) => SetAngleFeature.Function(angle,speed);
    }
}
