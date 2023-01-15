using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Machines
{
    /// <summary>
    /// 操作可能な全ての部品を表現する共通基底クラス
    /// </summary>
    public abstract class Module
    {
        protected Module(string name,string description)
        {
            Name = name;
            Description = description;
            Features = ImmutableArray<Feature<Delegate>>.Empty;
        }
        /// <summary>
        /// コンポーネントの名前
        /// </summary>
        /// <example>
        /// 例えば、四輪車の右前ホイールのモータ
        /// <code>
        /// Name == "RightFrontWheel"
        /// </code>
        /// </example>
        public string Name { get; private init; }
        /// <summary>
        /// コンポーネントの説明
        /// </summary>
        /// <example>
        /// 例えば、四輪車の右前ホイールのモータ
        /// <code>
        /// Description == "右前のホイールモータ"
        /// </code>
        /// </example>
        public string Description { get; private init; }
        /// <summary>
        /// 部品そのものの名前
        /// </summary>
        public abstract string ModuleName { get; }
        /// <summary>
        /// 部品そのものの説明
        /// </summary>
        public abstract string ModuleDescription { get; }
        /// <summary>
        /// 初期化処理を表現します。
        /// </summary>
        public virtual void Reset() { }
        /// <summary>
        /// そのモジュールが提供する全ての機能を返します。
        /// </summary>
        public ImmutableArray<Feature<Delegate>> Features { get; protected init; } 
    }
}
