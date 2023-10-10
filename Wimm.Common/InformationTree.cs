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
    public record InformationTree(string Name,ImmutableArray<InformationTree> Entries) : INotifyPropertyChanged
    {
        private string _value = string.Empty;
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
        private void Notify([CallerMemberName] string? name = null)
        {
            if (name is not null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
