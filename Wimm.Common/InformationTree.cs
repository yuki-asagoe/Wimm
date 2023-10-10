using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Wimm.Common
{
    /// <summary>
    /// モジュールが提供する情報を保存するコレクションです。
    /// 許可された場所以外からのアクセスを禁止します。(スレッドセーフでないため)
    /// </summary>
    public record InformationTree(string Name,ImmutableArray<InformationTree> Children,ImmutableArray<InformationTree.Entry> Entries)
    {
        /// <param name="IsReadOnly">これが<c>true</c>の時、Wimmはこのエントリーの値を書き換えません</param>
        public record Entry(string Name, bool IsReadOnly=true) : INotifyPropertyChanged
        {
            private string _value=string.Empty;
            public string Value
            {
                set
                {
                    _value = value;
                    Notify();
                }
                get
                {
                    return _value;
                }
            }
            public event PropertyChangedEventHandler? PropertyChanged;
            private void Notify([CallerMemberName]string? name=null)
            {
                if(name is not null)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                }
            }
        }
    }
}
