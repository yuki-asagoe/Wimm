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
    public record KeyBinding(ImmutableArray<Key> Keys,GamepadButtons GamepadKeys,Func<LuaResult> Action)
    {
        public bool IsActive(int gamepadIndex)
        {
            if (!XInput.GetState(gamepadIndex, out var padState)) return false;
            var gamepadPressed= (padState.Gamepad.Buttons & GamepadKeys) == GamepadKeys;
            if (!gamepadPressed) return false;
            return Keys.All(x => Keyboard.IsKeyDown(x));
        }
        public void Run() => Action();
        public void TryRun(int gamepadIndex)
        {
            if (IsActive(gamepadIndex)) Action();
        }
    }
}
