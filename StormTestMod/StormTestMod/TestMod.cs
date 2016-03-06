using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Storm.ExternalEvent;
using Storm.StardewValley;
using Storm.StardewValley.Event;
using Storm.StardewValley.Wrapper;

namespace StormTestMod
{
    [Mod(Author = "Zoryn Aaron", Name = "Test Mod", Version = 1.0d)]
    public class TestMod : DiskResource
    {
        [DllImport("kernel32")]
        static extern bool AllocConsole();

        public static Thread ConsoleThread;

        [Subscribe]
        public void InitializeCallback(InitializeEvent @event)
        {
            //ConsoleThread = new Thread(RunConsole);
            //ConsoleThread.Start();
        }

        [Subscribe]
        public void PlayerDamagedCallback(PlayerDamagedEvent @event)
        {
            Console.WriteLine(@event.Damager._GetName() + " damaged the player for " + @event.Damage + " HP");
        }

        public static void RunConsole()
        {
            AllocConsole();
            Console.WriteLine("My precious console!");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("I'm awaiting your `input`, senpai~");
            Console.ForegroundColor = ConsoleColor.Gray;

            while (true)
            {
                string input = Console.ReadLine();

                //if (input == "fast")
                    
                Thread.Sleep(1000 / 60); //Only do anything 60 times a second.
            }
        }
    }
}
