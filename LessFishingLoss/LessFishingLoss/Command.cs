using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingMod
{
    public struct Command
    {
        public String CommandName;
        public String CommandDesc;
        public String[] CommandArgs;
        public String[] CalledArgs;
    }

    public class EventArgsCommand : EventArgs
    {
        public EventArgsCommand(Command command)
        {
            Command = command;
        }
        public Command Command { get; private set; }
    }
}
