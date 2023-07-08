using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Wimm.Ui.Commands;

namespace Wimm.Ui.Records
{
    public class GeneratorEntry : INotifyPropertyChanged
    {
        Action Generator { get; }
        public String Name { get; }
        private bool done = false;
        public bool Done
        {
            get { return done; }
            set
            {
                done = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Done)));
            }
        }
        public ICommand GenerateCommand { get; } 

        public GeneratorEntry(string name, Action generator)
        {
            Name = name;
            Generator = generator;
            GenerateCommand = new DelegateCommand(
                () =>
                {
                    if (done) return;
                    if (MessageBox.Show($"[{Name}]の再生成を行います。既存の内容は上書きされますがよろしいですか？", "ファイルの再生成", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        Generator();
                        done = true;
                    }
                },
                () => !done
            );
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
