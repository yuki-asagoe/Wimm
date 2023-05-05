using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wimm.Machines;
using System.Windows.Media;
using Wimm.Model.Video;
using System.IO;
using Wimm.Model.Control;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Drawing;
using System.Reflection;
using Vortice.XInput;
using Wimm.Model.Console;
using System.Windows.Input;
using System.Diagnostics;
using Wimm.Model.Control.Script.Macro;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Wimm.Ui.Records;
using Wimm.Ui.Commands;
using System.Windows.Media.Imaging;
using Wimm.Model.Control.Script;

namespace Wimm.Ui.ViewModel
{
    public sealed class MachineControlViewModel:DependencyObject,IDisposable
    {
        public MachineControlViewModel(DirectoryInfo machineDirectory)
        {
            TerminalController = new TerminalController(Dispatcher,GetDefaultCommands());
            MachineDirectory = machineDirectory;
            CommandMacroStart = new MacroStartCommand(this);
            CommandMacroStop = new MacroStopCommand(this);
            CommandStartQRDetect = new DelegateCommand(() =>
            {
                if(VideoProcessor is not null)VideoProcessor.QRDetecting = true;
                QRDetectionRunning = true;
                TerminalController.Post("QRコード検出を開始します。");
            }, 
            () => { return VideoProcessor is not null && QRDetectionRunning is false; }
            );
            CommandStopQRDetect = new DelegateCommand(() =>
            {
                if (VideoProcessor is not null) VideoProcessor.QRDetecting = false;
                QRDetectionRunning = false;
                TerminalController.Post("QRコード検出を停止しました。");
            },
            () => { return VideoProcessor is not null && QRDetectionRunning is true; }
            );
            WimmFeatureProvider = new WimmFeatureProvider(this);
        }
        private DirectoryInfo MachineDirectory { get; init; }
        private WimmFeatureProvider WimmFeatureProvider { get; init; }
        public async Task<Exception?> OnLoad(HwndSource hwnd,FrameworkElement sizeObservedElement,Dispatcher dispatcher)
            //HwndSourceがWindowロード後しかアクセスできないのでここでMachine構築
        {
            (var e,var controller) = await Task.Run<(Exception?,MachineController?)>( 
                () =>
                {
                    MachineController? controller = null;
                    try
                    {
                        controller = MachineController
                            .Builder
                            .Build(
                                MachineDirectory,
                                new TpipConstructorArgs(hwnd, GeneralSetting.Default.Tpip_IP_Address),
                                WimmFeatureProvider,
                                TerminalController.GetLogger()
                            );
                    }
                    catch(TargetInvocationException e)
                    {
                        if(e.InnerException is Exception innerException)
                        {
                            return (innerException,null);
                        }
                        return (e, null);
                    }
                    catch(Exception e)
                    {
                        return (e, null);
                    }
                    return (null, controller);
                }
            );
            if(e is not null)
            {
                return e;
            }
            if(controller is null)
            {
                return new InvalidDataException("制御モデルの構築に失敗しました。");
            }
            MachineController = controller;
            var size = sizeObservedElement.RenderSize;
            VideoProcessor = new VideoProcessor(
                new System.Drawing.Size(600,800),
                MachineController.Machine.Camera
            );
            VideoProcessor.QRUpdated += (result) =>
                dispatcher.BeginInvoke(() => {
                    QRDetectionRunning = false;
                    DetectedQRCodeValue = result.Result;
                    if(result.DetectedArea is not null)DetectedQRCode = result.DetectedArea;
                    TerminalController.Post($"QRコードが検出されました。[{result.Result}]");
                });
            VideoProcessor.ImageUpdated += (image) =>
            {
                dispatcher.BeginInvoke(() => { CameraOutput = image; });
            };
            MachineName = MachineController.Machine.Name;
            foreach(var c in MachineController.Machine.Camera.Channels)
            {
                CameraChannelEntries.Add(new CameraChannelEntry(MachineController.Machine.Camera,c));
            }
            periodicTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle, dispatcher);
            periodicTimer.Tick += HighRatePeriodicWork;
            periodicTimer.Interval += new TimeSpan(0, 0, 0, 0, 500);
            periodicTimer.Start();
            IsControlRunning = true;
            MachineSpeedModifier = 1;
            TerminalController.Post($"ロボットの初期化が完了しました。ロボット名 : {MachineController.Machine.Name}");
            return null;
        }
        private void HighRatePeriodicWork(object? sender, EventArgs args)
        {
            ConnectionStatus = MachineController?.Machine?.ConnectionStatus ?? ConnectionState.Offline;
            if (MachineController is not null && XInput.GetState(MachineController.ObservedGamepadIndex, out var state))
            {
                ObservedGamepad = state.Gamepad;
            }
            if (MachineController is MachineController controller)
            {
                if (controller.IsMacroRunning) { 
                    ControlStatus = ControlStatus.Macro;
                    MacroProgress = controller.MacroRunningSecond;
                }
                else if (controller.IsControlStopping) { ControlStatus = ControlStatus.Idle; }
                else { ControlStatus = ControlStatus.Running; }
            }
            else ControlStatus = ControlStatus.Idle;
        }
        private void OnMachineSet()
        {
            (CommandMacroStart as MacroStartCommand)?.OnMachineSet();
            (CommandMacroStop as MacroStopCommand)?.OnMachineSet();
        }
        public void StartMacro(MacroInfo macro)
        {
            if (MachineController is MachineController controller)
            {
                controller.StartMacro(macro);
                ControlStatus = ControlStatus.Macro;
                MacroMaxProgress = controller.MacroMaxSecond;
                MacroProgress = 0;
                RunningMacro = macro;
            }
        }
        public void StopMacro()
        {
            MachineController?.StopMacro();
        }
        private record MacroStartCommand(MachineControlViewModel model) : ICommand
        {
            public event EventHandler? CanExecuteChanged;
            public void OnMachineSet() { CanExecuteChanged?.Invoke(null, new EventArgs()); }
            public bool CanExecute(object? parameter) => model.MachineController is not null;
            public void Execute(object? parameter){ if (parameter is MacroInfo info) model.StartMacro(info); }
        }
        private record MacroStopCommand(MachineControlViewModel model) : ICommand
        {
            public event EventHandler? CanExecuteChanged;
            public void OnMachineSet() { CanExecuteChanged?.Invoke(null, new EventArgs()); }
            public bool CanExecute(object? parameter) => model.MachineController is not null;
            public void Execute(object? parameter) { model.StopMacro(); }
        }

        private bool disposed;
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            controller?.Dispose();
            periodicTimer?.Stop();
            TerminalController.Dispose();
            GC.SuppressFinalize(this);
        }
        ~MachineControlViewModel()
        {
            if (!disposed) Dispose();
        }

        private MachineController? controller = null;
        private DispatcherTimer? periodicTimer = null;
        public ObservableCollection<CameraChannelEntry> CameraChannelEntries { get; } = new();
        public ICommand CommandMacroStart { get; }
        public ICommand CommandMacroStop { get; }
        public ICommand CommandStartQRDetect { get; }
        public ICommand CommandStopQRDetect { get; }
        public TerminalController TerminalController { get; }
        public MachineController? MachineController { get { return controller; } private set { controller = value;OnMachineSet(); } }
        private VideoProcessor? VideoProcessor { get; set; }
        public ICommand TerminalExecuteCommand => TerminalController.ExecuteCommand;
        public IEnumerable TerminalLines => TerminalController.LinesView;
        public readonly static DependencyProperty IsControlRunningProperty
            = DependencyProperty.Register(
                "IsControlRunning", typeof(bool), typeof(MachineControlViewModel),
                new PropertyMetadata((DependencyObject property,DependencyPropertyChangedEventArgs args) => {
                    if(property is MachineControlViewModel model && args.NewValue is bool value)
                    {
                        if (value){
                            model.MachineController?.StartControlLoop();
                        }else{
                            model.MachineController?.StopControlLoop();
                        }
                    }
                })
            );
        public readonly static DependencyProperty MachineSpeedModifierProperty
            = DependencyProperty.Register(
                "MachineSpeedModifier", typeof(double), typeof(MachineControlViewModel),
                new PropertyMetadata((property, args) => { 
                    if(property is MachineControlViewModel model && model.MachineController?.Machine is Machine machine && args.NewValue is double value)
                    {
                        machine.SpeedModifier = value;
                    }
                })
            );
        public readonly static DependencyProperty MachineNameProperty
            = DependencyProperty.Register("MachineName", typeof(string), typeof(MachineControlViewModel));
        public readonly static DependencyProperty ConnectionStatusProperty
            = DependencyProperty.Register("ConnectionStatus", typeof(ConnectionState), typeof(MachineControlViewModel));
        public readonly static DependencyProperty QRDetectionRunningProperty
            = DependencyProperty.Register("QRDetectionRunning", typeof(bool), typeof(MachineControlViewModel));
        public readonly static DependencyProperty DetectedQRCodeValueProperty
            = DependencyProperty.Register("DetectedQRCodeValue", typeof(string), typeof(MachineControlViewModel));
        public readonly static DependencyProperty DetectedQRCodeProperty
            = DependencyProperty.Register("DetectedQRCode", typeof(BitmapSource), typeof(MachineControlViewModel));
        public readonly static DependencyProperty CameraOutputProperty
            = DependencyProperty.Register("CameraOutput", typeof(ImageSource), typeof(MachineControlViewModel));
        public readonly static DependencyProperty ObservedGamepadProperty
            = DependencyProperty.Register("ObservedGamepad", typeof(Gamepad), typeof(MachineControlViewModel));
        public readonly static DependencyProperty RunningMacroProperty
            = DependencyProperty.Register("RunningMacro", typeof(MacroInfo), typeof(MachineControlViewModel));
        public readonly static DependencyProperty MacroMaxProgressProperty
            = DependencyProperty.Register("MacroMaxProgress", typeof(double), typeof(MachineControlViewModel));
        public readonly static DependencyProperty MacroProgressProperty
            = DependencyProperty.Register("MacroProgress", typeof(double), typeof(MachineControlViewModel));
        public readonly static DependencyProperty ControlStateProperty
            = DependencyProperty.Register("ControlStatus", typeof(ControlStatus), typeof(MachineControlViewModel));

        public ControlStatus ControlStatus
        {
            get { return (ControlStatus)GetValue(ControlStateProperty); }
            set { SetValue(ControlStateProperty, value); }
        }
        public double MachineSpeedModifier
        {
            get { return (double)GetValue(MachineSpeedModifierProperty); }
            set { SetValue(MachineSpeedModifierProperty, value); }
        }
        public MacroInfo RunningMacro
        {
            get { return (MacroInfo)GetValue(RunningMacroProperty); }
            set { SetValue(RunningMacroProperty, value); }
        }
        public double MacroProgress
        {
            get { return (double)GetValue(MacroProgressProperty); }
            set { SetValue(MacroProgressProperty, value); }
        }
        public double MacroMaxProgress
        {
            get { return (double)GetValue(MacroMaxProgressProperty); }
            set { SetValue(MacroMaxProgressProperty, value); }
        }
        public Gamepad ObservedGamepad
        {
            get { return (Gamepad)GetValue(ObservedGamepadProperty); }
            set { SetValue(ObservedGamepadProperty, value); }
        }
        public string MachineName
        {
            get { return (string)GetValue(MachineNameProperty); }
            set { SetValue(MachineNameProperty, value); }
        }
        public ConnectionState ConnectionStatus
        {
            get { return (ConnectionState)GetValue(ConnectionStatusProperty); }
            private set { SetValue(ConnectionStatusProperty,value); }
        }
        public bool QRDetectionRunning
        {
            get { return (bool)GetValue(QRDetectionRunningProperty); }
            set { SetValue(QRDetectionRunningProperty, value); }
        }
        public string DetectedQRCodeValue
        {
            get { return GetValue(DetectedQRCodeValueProperty) as string ?? ""; }
            private set { SetValue(DetectedQRCodeValueProperty, value); }
        }
        public BitmapSource DetectedQRCode
        {
            get { return (BitmapSource)GetValue(DetectedQRCodeProperty); }
            private set { SetValue(DetectedQRCodeProperty, value); }
        }
        public ImageSource? CameraOutput
        {
            get { return GetValue(CameraOutputProperty) as ImageSource; }
            private set { if(value is not null)SetValue(CameraOutputProperty, value); }
        }
        public bool IsControlRunning
        {
            get { return (bool)GetValue(IsControlRunningProperty); }
            set { SetValue(IsControlRunningProperty, value); }
        }

        private CommandNode[] GetDefaultCommands()
        {
            return new CommandNode[]
            {
                new CommandNode("camera",
                    new[]
                    {
                        new CommandNode(
                            "activate",Array.Empty<CommandNode>(),
                            new[]{new KeyValuePair<string,Type>("number",typeof(int))},
                            (param) =>{
                                if(MachineController is not null && param.Count >= 1 && int.TryParse(param[0],out var index))
                                {
                                    if (0 <= index && index < CameraChannelEntries.Count)
                                    {
                                        CameraChannelEntries[index].IsActive=true;
                                    }
                                    TerminalController.Post($"{index}番カメラのアクティブ化" +
                                        $"を要求しました");
                                }
                                else{ TerminalController.Post("引数のパースに失敗。"); }
                            }
                        )
                    }
                ),
                new CommandNode("qr",
                    new[]
                    {
                        new CommandNode(
                            "detect",Array.Empty<CommandNode>(),Array.Empty<KeyValuePair<string,Type>>(),
                            (param) =>
                            {
                                if (QRDetectionRunning)
                                {
                                    TerminalController.Post("QRコード検出は実行中です。");
                                }
                                else
                                {
                                    if(CommandStartQRDetect.CanExecute(null))CommandStartQRDetect.Execute(null);
                                }
                            }
                        ),
                        new CommandNode(
                            "stop",Array.Empty<CommandNode>(),Array.Empty<KeyValuePair<string,Type>>(),
                            (param) =>
                            {
                                if (!QRDetectionRunning)
                                {
                                    TerminalController.Post("QRコード検出は実行されていません。");
                                }
                                else
                                {
                                    if(CommandStopQRDetect.CanExecute(null))CommandStartQRDetect.Execute(null);
                                }
                            }
                        )
                    }
                ),
                new CommandNode("timer",
                    new[]
                    {
                        new CommandNode(
                            "set",Array.Empty<CommandNode>(),
                            new []{new KeyValuePair<string,Type>("number",typeof(int))},
                            (param)=>{
                            }
                        )
                    }
                ),
                new CommandNode("exit",
                    Array.Empty<CommandNode>(),
                    Array.Empty<KeyValuePair<string,Type>>(),
                    (_)=>{}
                )
            };
        }
    }
}
