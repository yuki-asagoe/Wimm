using ObservableCollections;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Wimm.Machines;
using Wimm.Model.Control;
using Wimm.Ui.Commands;

namespace Wimm.Ui.Component
{
    public class FeatureExecutionManager : INotifyPropertyChanged
    {
        public FeatureExecutionManager()
        {
            CallFeatureCommand = new DelegateCommand(
                async () =>
                {
                    if(CanExecute())
                    {
                        var info = GetExcutionInfo();
                        if(info.HasValue)
                        {
                            var nonNullInfo = info.Value;
                            var result = await Controller!.RequestExcutionAsync(nonNullInfo.Item1, nonNullInfo.args);
                            if(result.returnedValue is not null)
                            {
                                ReturnValue = result.returnedValue;
                            }
                            else
                            {
                                ReturnValue = $"{result.e?.GetType()?.Name}:{result.e?.Message}";
                            }
                        }
                    }
                },
                () => CanExecute()
            );
        }
        public ICommand CallFeatureCommand { get; }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void Notify([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(property));
        }
        public MachineController? Controller { private get; set; } = null;
        Feature<Delegate>? feature = null;
        public Feature<Delegate>? Feature
        {
            get { return feature; }
            set
            {
                if (value == feature) return;
                feature = value;
                if (value is null)
                {
                    Parameters = ImmutableArray<Parameter>.Empty;
                }
                else 
                {
                    Parameters = value.Function.Method.GetParameters().Select(it=>new Parameter(it)).ToImmutableArray();
                }
                Notify();
            }
        }
        object? returned = null;
        public object? ReturnValue
        {
            get { return returned; }
            set { returned = value; Notify(); }
        }
        ImmutableArray<Parameter> parameters = ImmutableArray<Parameter>.Empty;
        public ImmutableArray<Parameter> Parameters
        {
            get { return parameters; }
            set { parameters = value; Notify(); }
        }
        private bool CanExecute()
        {
            if (Feature is null) return false;
            if (Parameters.Any(it => it.Value is null)) return false;
            if (Controller is null) return false;
            return true;
        }
        public (Feature<Delegate>, object[]? args)? GetExcutionInfo()
        {
            if (!CanExecute()) return null;
            if (Feature is null) return null;
            return (Feature, Parameters.Length == 0 ? null : Parameters.Select(it=>it.Value!).ToArray());
        }
    }
    public record Parameter(ParameterInfo ParameterInfo) : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private static readonly Type[] AcceptableTypes = new[]
        {
            typeof(int),typeof(short),typeof(long),typeof(string),typeof(byte),typeof(float),typeof(double)
        };
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        private void Notify([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        private string? errorText = null;
        private string? Error
        {
            set
            {
                errorText = value;
                ErrorsChanged?.Invoke(this,new DataErrorsChangedEventArgs(nameof(ValueAsString)));
            }
        }
        public IEnumerable GetErrors(string? propertyName)
        {
            if(errorText is null || propertyName == nameof(ValueAsString))
            {
                return Array.Empty<string>();
            }
            else
            {
                return new string[] { errorText };
            }
        }

        public bool HasErrors => errorText is not null;

        public string Name { get; } = ParameterInfo.Name;
        public Type Type { get; } = ParameterInfo.ParameterType;
        public bool CanAccept { get; } = AcceptableTypes.Contains(ParameterInfo.ParameterType);
        string valueAsString = string.Empty;
        public string ValueAsString
        {
            get { return valueAsString; }
            set
            {
                valueAsString = value;
                #region type convert
                if (Type == typeof(int))
                {
                    if (int.TryParse(value, out int number))
                    {
                        Value = number;
                    }
                    else
                    {
                        Value = null;
                        Error = "Cannot convert to int";
                    }
                }
                else if(Type == typeof(short))
                {
                    if (short.TryParse(value, out short number))
                    {
                        Value = number;
                    }
                    else
                    {
                        Value = null;
                        Error = "Cannot convert to short";
                    }
                }
                else if (Type == typeof(byte))
                {
                    if (byte.TryParse(value, out byte number))
                    {
                        Value = number;
                    }
                    else
                    {
                        Value = null;
                        Error = "Cannot convert to byte";
                    }
                }
                else if (Type == typeof(long))
                {
                    if (long.TryParse(value, out long number))
                    {
                        Value = number;
                    }
                    else
                    {
                        Value = null;
                        Error = "Cannot convert to long";
                    }
                }
                else if (Type == typeof(string))
                {
                    Value = value;
                }
                else if (Type == typeof(float))
                {
                    if(float.TryParse(value,out float number))
                    {
                        Value = number;
                    }
                    else
                    {
                        Value = null;
                        Error = "Cannot convert to float";
                    }
                }
                else if (Type == typeof(double))
                {
                    if(double.TryParse(value,out double number))
                    {
                        Value = number;
                    }
                    else
                    {
                        Value = null;
                        Error = "Cannot convert to double";
                    }
                }
                else
                {
                    Value = null;
                    Error = $"Type[{Type.Name}] is not supported";
                }
                #endregion
                Notify();
            }
        }
        object? value = null;
        public object? Value
        {
            get { return value; }
            private set { this.value = value; Notify(); }
        }
    }
}
