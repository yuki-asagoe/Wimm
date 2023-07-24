using ObservableCollections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Wimm.Logging;

namespace Wimm.Model.Console
{
    public class TerminalController:IDisposable
    {
        private class OutputFormatter : IEnumerable<string>, INotifyCollectionChanged
        {
            #region OutputFormatter
            INotifyCollectionChangedSynchronizedView<string, string> Lines;
            public OutputFormatter(INotifyCollectionChangedSynchronizedView<string, string> output)
            {
                Lines = output;
                output.CollectionChanged += (_, args) => { this.CollectionChanged?.Invoke(this, args); };
            }
            public event NotifyCollectionChangedEventHandler? CollectionChanged;
            private class Enumerator : IEnumerator<string>
            {
                IEnumerator<(string, string)> InEnumerator;
                public Enumerator(IEnumerator<(string,string)> enumerator)
                {
                    InEnumerator = enumerator;
                }
                public string Current => InEnumerator.Current.Item2;

                object IEnumerator.Current => Current;

                public void Dispose()
                {
                    InEnumerator.Dispose();
                }

                public bool MoveNext()
                {
                    return InEnumerator.MoveNext();
                }

                public void Reset()
                {
                    InEnumerator.Reset();
                }
            }
            public IEnumerator<string> GetEnumerator()
            {
                return new Enumerator(Lines.GetEnumerator());
            }

            IEnumerator IEnumerable.GetEnumerator(){ return GetEnumerator(); }
            #endregion
        }
        private StreamWriter? LogOutput { get; }
        private FileInfo? LogFile { get; }
        private const int LineSize = 50;
        public SortedList<string, CommandNode> RootCommands;
        public TerminalExecuteCommand ExecuteCommand { get; }
        public ObservableFixedSizeRingBuffer<string> Lines { get; } = new ObservableFixedSizeRingBuffer<string>(LineSize);
        private ISynchronizedView<string,string> View { get; }
        public IEnumerable Output { get; }
        public TerminalController(IEnumerable<CommandNode> commands)
        {
            View = Lines.CreateView(x => x);
            var output= View.WithINotifyCollectionChanged();
            BindingOperations.EnableCollectionSynchronization(output, new object());
            Output = new OutputFormatter(output);

            LogFile = GetLogFile();
            if(LogFile is not null)
            {
                LogOutput = new StreamWriter(LogFile.Open(FileMode.OpenOrCreate, FileAccess.Write),Encoding.UTF8);
            }
            RootCommands = new SortedList<string, CommandNode>();
            foreach(var i in commands)
            {
                RootCommands.Add(i.Name, i);
            }
            ExecuteCommand = new TerminalExecuteCommand(this);
        }
        public void EnterCommand(string commandText)
        {
            var commandNodes = commandText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var result = SearchCommand(commandNodes, null, RootCommands, 0);
            Post(commandText, MessageLevel.Input);
            if(result.HasValue)
            {
                var value = result.Value;
                var action = value.Item1.Action;
                if(action is not null) { action(value.Item2); }
                else { Post("コマンドが実行可能ではありませんでした。", MessageLevel.Warning); }
            }
            else
            {
                Post("コマンドが見つかりませんでした。", MessageLevel.Warning);
            }
        }
        private (CommandNode, string[])? SearchCommand(string[] commandNodes,CommandNode? parent,SortedList<string,CommandNode> commandList,int index)
        {
            if (commandNodes.Length <= index) return null;
            var node = commandNodes[index];
            if(commandList.TryGetValue(node,out var command))
            {
                var returned=SearchCommand(commandNodes, command,command.Children, index + 1);
                if(returned is not null) { return returned; }
            }
            if (parent is null)
            {
                return null;
            }
            else if(commandNodes.Length - index == parent.ParamsHint.Count)//引数の個数が正しいことを確認
            {
                return (parent, commandNodes.Length > index ? commandNodes[index..] : Array.Empty<string>());
            }
            else
            {
                return null;
            }
        }
        public void Post(string text,MessageLevel level=MessageLevel.Info)
        {
            var message = level is MessageLevel.Input?$"> {text}":$"[{level}]{text}";
            LogOutput?.WriteLine($"[{DateTime.Now}]{message}");

            if (Lines.Count == LineSize)
            {
                 Lines.RemoveFirst();
             }
            Lines.AddLast(message);
        }
        public static FileInfo? GetLogFile()
        {
            var logDir = ILogger.GetLogDirectory();
            if(logDir is not null)
            {
                var date = DateTime.Now;
                return new FileInfo(logDir.FullName + $"/{date.Year}-{date.Month}-{date.Day}_{date.Hour}-{date.Minute}-{date.Second}.log");
            }
            else
            {
                return null;
            }
        }
        public ILogger GetLogger()
        {
            return new TerminalLogger(this);
        }

        public void Dispose()
        {
            LogOutput?.Close();
            View.Dispose();
            GC.SuppressFinalize(this);
        }
        ~TerminalController()
        {
            Dispose();
        }
        public class TerminalExecuteCommand : ICommand
        {
            readonly TerminalController Controller;
            public TerminalExecuteCommand(TerminalController controller)
            {
                Controller = controller;
            }
            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                return parameter is string;
            }

            public void Execute(object? parameter)
            {
                if(parameter is string input)
                {
                    Controller.EnterCommand(input);
                }
            }
        }
        private class TerminalLogger : ILogger
        {
            private readonly TerminalController Controller;
            public TerminalLogger(TerminalController controller)
            {
                Controller = controller;
            }
            public void Info(String message) {
                Controller.Post(message, MessageLevel.Info);
            }
            public void Warn(String message) {
                Controller.Post(message, MessageLevel.Warning);
            }
            public void Error(String message) {
                Controller.Post(message, MessageLevel.Error);
            }
        }
    }
    public enum MessageLevel
    {
        Info,Warning,Error,Input
    }
}
