using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Wimm.Machines;
using Wimm.Ui.Commands;
using Wimm.Ui.Records;

namespace Wimm.Ui.ViewModel
{
    public class MachineConfigEditViewModel:DependencyObject
    {
        public static readonly DependencyProperty ConfigEntriesProperty
            = DependencyProperty.Register("ConfigEntries", typeof(MachineConfigEntry[]), typeof(MachineConfigEditViewModel));
        public static readonly DependencyProperty MachineNameProperty
            = DependencyProperty.Register("MachineName", typeof(string), typeof(MachineConfigEditViewModel));
        public static readonly DependencyProperty FeedbackProperty
            = DependencyProperty.Register("Feedback", typeof(string), typeof(MachineConfigEditViewModel));
        public MachineConfigEntry[] ConfigEntries
        {
            get { return (MachineConfigEntry[])GetValue(ConfigEntriesProperty); }
            set { SetValue(ConfigEntriesProperty, value); }
        }
        public string MachineName
        {
            get { return (string)GetValue(MachineNameProperty); }
            set { SetValue(MachineNameProperty, value); }
        }
        public string Feedback
        {
            get { return (string)GetValue(FeedbackProperty); }
            set { SetValue(FeedbackProperty, value); }
        }
        public ICommand SaveCommand { get; init; }
        public ICommand ResetCommand { get; init; }
        private FileInfo ConfigFile { get; init; }
        public MachineConfigEditViewModel(string machineName,FileInfo configFile)
        {
            MachineName = machineName;
            ConfigFile = configFile;
            try
            {
                var jsonReader = new Utf8JsonReader(new ReadOnlySpan<byte>(File.ReadAllBytes(ConfigFile.FullName)));
                var items = MachineConfig.LoadConfigJson(jsonReader);
                ConfigEntries = items.Select((it) => { return new MachineConfigEntry(it.Key, it.Value.Value, it.Value.Default); }).ToArray();
            }
            catch(IOException e)
            {
                Feedback = $"Error:ファイルを開けませんでした。[{ConfigFile.FullName}]";
            }
            if(ConfigEntries is null)
            {
                ConfigEntries = Array.Empty<MachineConfigEntry>();
            }
            SaveCommand = new DelegateCommand(
                () =>
                {
                    try
                    {
                        using var writer = new Utf8JsonWriter(ConfigFile.OpenWrite());
                        MachineConfig.WriteConfigJson(
                            writer,
                            ConfigEntries.Select(it => new KeyValuePair<string, (string, string?)>(it.Name, (it.Value, it.Default))).ToArray()
                        );
                        Feedback = "変更を保存しました。";
                    }
                    catch(IOException e)
                    {
                        Feedback = $"Error:ファイルを開けませんでした。[{ConfigFile.FullName}]";
                    }
                }
            );
            ResetCommand = new DelegateCommand(
                () =>
                {
                    foreach(var item in ConfigEntries)
                    {
                        if(item.Default is not null)
                        {
                            item.Value = item.Default;
                        }
                    }
                    Feedback = "初期値に設定しました。";
                }
            );
        }
    }
}
