using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Wimm.Machines.Video;
using Wimm.Ui.Commands;

namespace Wimm.Ui.Records
{
    public class CameraChannelEntry : INotifyPropertyChanged
    {
        public CameraChannelEntry(Camera camera,Channel channel)
        {
            Channel = channel;
            channel.ActivationChanged += (active) => { IsActive = active; };
            name = channel.Name;
            Camera = camera;
            Index = Camera.Channels.IndexOf(channel);
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void Notify([CallerMemberName]string? name = null) {  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        private string name;
        private bool active;
        private Channel Channel { get; }
        private Camera Camera { get; }
        public int Index { get; }
        public string Name { get { return name; } set { name = value;Notify(); } }
        public bool IsActive { get { return active; } set { if (active != value) { active = value; Camera.Activate(Channel, value); Notify(); } } }
    }
}
