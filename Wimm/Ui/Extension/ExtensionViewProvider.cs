using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wimm.Machines.Extension;

namespace Wimm.Ui.Extension
{
    public abstract class ExtensionViewProvider
    {
        protected ExtensionViewProvider(IMachineExtension instance)
        {
            ExtentionInstance = instance;
        }
        protected IMachineExtension ExtentionInstance { get; init; }
        public abstract FrameworkElement View { get; init; }
        public abstract string Name { get; }
        public abstract void OnPeriodicTimer();
    }
    [AttributeUsage(AttributeTargets.Class)]
    internal class ExtensionProviderAttribute : Attribute
    {
        public Type TargetType { get; init; }
        public ExtensionProviderAttribute(Type target)
        {
            TargetType = target;
            if (!target.IsAssignableTo(typeof(IMachineExtension)))
            {
                throw new ArgumentException($"型[{target.FullName}]が{nameof(IMachineExtension)}のサブクラスではありません");
            }
        }
    }
}
