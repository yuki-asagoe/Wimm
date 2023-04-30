using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.XInput;

namespace Wimm.Model.Control.Script
{
    public sealed class InputSupporter
    {
        public bool IsRightThumbUp(Gamepad pad)
        {
            return Gamepad.RightThumbDeadZone < pad.RightThumbY;
        }
        public bool IsRightThumbDown(Gamepad pad)
        {
            return -Gamepad.RightThumbDeadZone > pad.RightThumbY;
        }
        public bool IsRightThumbRight(Gamepad pad)
        {
            return Gamepad.RightThumbDeadZone < pad.RightThumbX;
        }
        public bool IsRightThumbLeft(Gamepad pad)
        {
            return -Gamepad.RightThumbDeadZone > pad.RightThumbX;
        }
        public bool IsLeftThumbUp(Gamepad pad)
        {
            return Gamepad.LeftThumbDeadZone < pad.LeftThumbY;
        }
        public bool IsLeftThumbDown(Gamepad pad)
        {
            return -Gamepad.LeftThumbDeadZone > pad.LeftThumbY;
        }
        public bool IsLeftThumbRight(Gamepad pad)
        {
            return Gamepad.LeftThumbDeadZone < pad.LeftThumbX;
        }
        public bool IsLeftThumbLeft(Gamepad pad)
        {
            return -Gamepad.LeftThumbDeadZone > pad.LeftThumbX;
        }
        public bool IsButtonDown(Gamepad pad, int button)
        {
            return (pad.Buttons & (GamepadButtons)button) != GamepadButtons.None;
        }
    }
}
