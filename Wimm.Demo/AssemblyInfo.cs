using Wimm.Common;
using Wimm.Demo.Machine;

[assembly:LoadTarget(typeof(DemoMachine))]
[assembly:BuiltInResource(ResourceType.ScriptInitialize,"Wimm.Demo.Machine.Resources.Script.initialize.lua")]
[assembly: BuiltInResource(ResourceType.Icon, "Wimm.Demo.Machine.Resources.icon.png")]
[assembly: BuiltInResource(ResourceType.Description, "Wimm.Demo.Machine.Resources.description.txt")]