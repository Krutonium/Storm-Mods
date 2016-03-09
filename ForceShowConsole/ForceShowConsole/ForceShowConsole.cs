using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Storm.ExternalEvent;
using Storm.StardewValley.Event;

namespace ForceShowConsole
{
    [Mod]
    public class ForceShowConsole
    {
        [DllImport("kernel32")]
        static extern bool AllocConsole();
        public static Thread ConsoleThread;

        public void Init(InitializeEvent @event)
        {
            AllocConsole();

            Console.WriteLine("Console Shower by Zoryn => Initialization Completed");
        }
    }
}
