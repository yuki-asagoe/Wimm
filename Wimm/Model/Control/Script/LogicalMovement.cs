using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Model.Control.Script
{
    public class LogicalMovement // TODO
    {
        public Action<double>? MoveForwardAction { get; set; }
        public Action<double>? MoveRightwardAction { get; set; }
        public Action<double>? MoveUpwardAction { get; set; }
        public Action<double>? TurnRightwardAction { get; set; }
    }
}
