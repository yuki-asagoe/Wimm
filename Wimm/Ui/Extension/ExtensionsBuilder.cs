using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wimm.Machines;
using Wimm.Machines.Extension;
using Wimm.Ui.Extension.Provider;

namespace Wimm.Ui.Extension
{
    internal class ExtensionsBuilder
    {
        private static ExtensionsBuilder? instance = null;
        private static ImmutableArray<Type> ProviderTypes
        {
            get => ImmutableArray.Create(
                typeof(PowerVoltageProvider),
                typeof(BatteryLevelProvider)
            );
        }
        public static ExtensionsBuilder Instance {
            get { instance ??= new ExtensionsBuilder(); return instance; }
        }
        private ExtensionsBuilder()
        {
            foreach(var type in ProviderTypes)
            {
                if (!type.IsSubclassOf(typeof(ExtensionViewProvider)))
                {
                    throw new Exception($"型[{type.FullName}]が[{nameof(ExtensionViewProvider)}]のサブクラスではありません。開発者に報告してください。");
                }
                if (type.GetConstructor(new[] { typeof(IMachineExtension) } ) is null)
                {
                    throw new Exception($"型[{type.FullName}]が引数({nameof(IMachineExtension)})のコンストラクタを実装しません。開発者に報告してください。");
                }
                if(type.GetCustomAttribute<ExtensionProviderAttribute>() is null)
                {
                    throw new Exception($"型[{type.FullName}]に[{nameof(ExtensionProviderAttribute)}]属性が付与されていません。開発者に報告してください。");
                }
            }
        }
        public ImmutableArray<ExtensionViewProvider> Build(Machine machine)
        {
            var list = new LinkedList<ExtensionViewProvider>();
            foreach (var type in ProviderTypes)
            {
                if(type.GetCustomAttribute<ExtensionProviderAttribute>() is ExtensionProviderAttribute attribute)
                {
                    if (machine is IMachineExtension extension && extension.GetType().IsAssignableTo(attribute.TargetType))
                    {
                        list.AddLast((ExtensionViewProvider)type.GetConstructor(new[] { typeof(IMachineExtension) })!.Invoke(new object[] { extension }));
                    }
                }
            }
            return list.ToImmutableArray();
        }
    }
}
