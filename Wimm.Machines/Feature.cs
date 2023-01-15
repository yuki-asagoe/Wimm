using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Machines
{
    /// <summary>
    /// モジュールが提供する機能を表現するクラス
    /// </summary>
    /// <remarks>
    /// 最終的にDLRやスクリプトから呼び出されます。
    /// このクラスが提供する機能はControlProcessを開始してから終了するまでの間で呼び出す必要があります。
    /// </remarks>
    public sealed class Feature<F> where F : Delegate
    {
        public Feature(string name,string description,F function)
        {
            Name = name;
            Description = description;
            Function = function;
        }
        /// <summary>
        /// スクリプト側での識別に用いる名前、実際に格納している関数の名前とは異なります。
        /// </summary>
        /// <remarks>
        /// 全て小文字であることをおすすめします。
        /// </remarks>
        public string Name { get;private init; }
        public string Description { get; private init; }
        public F Function { get; private init; }
        public Feature<Delegate> CastToBaseForm()
        {
            return new Feature<Delegate>(Name,Description,Function);
        }
    }
}
