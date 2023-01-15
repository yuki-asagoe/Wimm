using Neo.IronLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Vortice.XInput;

namespace Wimm.Model.Control.Script
{
    public static class LuaKeysEnum
    {
        private static LuaTable? keyboardEnum=null;
        public static LuaTable KeyboardEnum
        {
            get
            {
                if(keyboardEnum is null)
                {
                    keyboardEnum = new LuaTable();
                    foreach(var key in Enum.GetValues<Key>())
                    {
                        keyboardEnum.TryAdd(key.ToString(),(int)key);
                    }
                }
                return keyboardEnum;
            }
        }
        private static LuaTable? gamepadKeyEnum =null;
        public static LuaTable GamepadKeyEnum
        {
            get
            {
                if(gamepadKeyEnum is null)
                {
                    gamepadKeyEnum = new LuaTable();
                    foreach(var button in Enum.GetValues<GamepadButtons>())
                    {
                        gamepadKeyEnum.TryAdd(button.ToString(), (int)button);
                    }
                }
                return gamepadKeyEnum;
            }
        }
    }
}
