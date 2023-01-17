using Neo.IronLua;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Vortice.XInput;

namespace Wimm.Model.Control
{
    public record KeyBinding(GamepadButtons GamepadKeys,Func<LuaResult> Action)
    {
        public bool IsActive(int gamepadIndex)
        {
            if (!XInput.GetState(gamepadIndex, out var padState)) return false;
            return (padState.Gamepad.Buttons & GamepadKeys) == GamepadKeys;
        }
        public void Run() => Action();
        public void TryRun(int gamepadIndex)
        {
            if (IsActive(gamepadIndex)) Action();
        }
    }
}
