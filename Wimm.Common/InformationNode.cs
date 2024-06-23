using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Wimm.Common
{
    /// <summary>
    /// モジュールが提供する情報を保存するコレクションです。
    /// </summary>
    public record InformationNode(string Name,ImmutableArray<InformationNode> Entries) : INotifyPropertyChanged
    {
        private bool valueChanged = false;
        private string _value = string.Empty;
        public string Value
        {
            set
            {
                _value = value;
                valueChanged = true;
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
        /// <summary>
        /// 値が変更されていた場合に<code>PropertyChanged</code>イベントを発行します。
        /// </summary>
        public void CheckUpdates()
        {
            if(valueChanged)
            {
                valueChanged = false;
                Notify(nameof(Value));
            }
            foreach(var child in Entries) { child.CheckUpdates(); }
        }
        /// <summary>
        /// 子要素から名前が一致するものを検索します
        /// </summary>
        public InformationNode? this[string name]
        {
            get{ return Entries.FirstOrDefault(it => it.Name == name); }
        }
    }
}
