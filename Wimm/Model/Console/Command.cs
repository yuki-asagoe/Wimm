using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Model.Console
{
    public class CommandNode
    {
        public CommandNode(string name, IList<CommandNode> children, IList<KeyValuePair<string, Type>>? paramHint=null, Action<IList<string>>? action = null)
        {
            Name = name;
            Children = new SortedList<string, CommandNode>(children.Count);
            foreach (var i in children) { Children.Add(i.Name, i); }
            ParamsHint = paramHint ?? new List<KeyValuePair<string, Type>>();
            Action = action;
        }
        public string Name { get; }
        public IList<KeyValuePair<string, Type>> ParamsHint { get; }
        public SortedList<string, CommandNode> Children { get; }
        /// <summary>
        /// コマンドを処理する関数 IListはパラメータ
        /// </summary>
        public Action<IList<string>>? Action { get; }
    }
}
