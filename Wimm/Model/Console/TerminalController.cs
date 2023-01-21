using ObservableCollections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace Wimm.Model.Console
{
    public class TerminalController:IDisposable
    {
        private StreamWriter? LogOutput { get; }
        private FileInfo? LogFile { get; }
        private const int LineSize = 50;
        public SortedList<string, CommandNode> RootCommands;
        public TerminalExecuteCommand ExecuteCommand { get; }
        public ObservableFixedSizeRingBuffer<string> Lines { get; } = new ObservableFixedSizeRingBuffer<string>(LineSize);
        private ISynchronizedView<string,string> View { get; }
        public INotifyCollectionChangedSynchronizedView<string,string> LinesView { get; }
        public TerminalController(IEnumerable<CommandNode> commands)
        {
            View = Lines.CreateView(x => x);
            LinesView= View.WithINotifyCollectionChanged();

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
            else if(commandNodes.Length - index == parent.ParamsHint.Count)//引数の戸数が正しいことを確認
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
            var entryLocation = Assembly.GetEntryAssembly()?.Location;
            if (entryLocation is null) return null;
            var entryFile = new FileInfo(entryLocation);
            var entryDir = new DirectoryInfo(entryFile.FullName[..^entryFile.Name.Length]);
            var logDir = new DirectoryInfo(entryDir.FullName + "/log");
            if (!logDir.Exists) logDir.Create();
            return new FileInfo(logDir.FullName + "/latest.log");
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
    }
    public enum MessageLevel
    {
        Info,Warning,Error,Input
    }
}
