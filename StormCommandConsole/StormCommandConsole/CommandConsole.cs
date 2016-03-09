using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using Storm.ExternalEvent;
using Storm.StardewValley;
using Storm.StardewValley.Event;
using Storm.StardewValley.Event.Game;
using Storm.StardewValley.Wrapper;
using Object = Storm.StardewValley.Wrapper.Object;

namespace StormCommandConsole
{
    [Mod(Author = "Zoryn Aaron", Name = "#Storm Command Console", Version = 1.0d)]
    public sealed class CommandConsole : DiskResource
    {
        [DllImport("kernel32")]
        static extern bool AllocConsole();
        public static Thread ConsoleThread;

        public static StaticContext Game
        {
            get { return StaticGameContext.WrappedGame; }
        }

        public static Farmer Player
        {
            get { return Game.Player; }
        }

        public static Form StardewForm { get; private set; }

        public const string AssemblyName = "#StormCommandConsole";
        public const string CurrentConfigGuid = "C5FB9F7B-0F2A-4DD7-9AF1-F5A8FD43C33C";

        public static string ExecutionLocation { get; private set; }
        public static string ConfigLocation { get; private set; }
        public static Config ModConfig { get; private set; }

        [Subscribe]
        public void InitializeCallback(InitializeEvent @event)
        {
            Console.Title = "Storm Modding API - CommandConsole by Zoryn";
            ExecutionLocation = ParentPathOnDisk;
            ConfigLocation = Path.Combine(ExecutionLocation, AssemblyName + ".Config.json");
            LoadConfig();

            FileInfo spath = new FileInfo(StormAPI.ModsPath + "\\" + AssemblyName + "\\" + AssemblyName + ".dll");
            FileInfo tpath = new FileInfo(Environment.CurrentDirectory + "\\" + AssemblyName + ".dll");

            LogInfo("SOURCE DLL: " + spath);
            LogInfo("TARGET DLL: " + tpath);

            if (tpath.Exists)
            {
                LogInfo("TARGET DLL EXISTS");
                if (spath.LastWriteTime > tpath.LastWriteTime)
                {
                    LogInfo("DLL IS OUTDATED - UPDATING...");
                    try
                    {
                        spath.CopyTo(tpath.FullName, true);
                    }
                    catch (Exception ex)
                    {
                        if (ModConfig.AutoUpdateWithoutPrompt)
                        {
                            Process.Start(new ProcessStartInfo()
                            {
                                Arguments = "/C choice /C Y /N /D Y /T 3 & Del \"" + tpath.FullName + "\" & Copy /V /Y \"" + spath.FullName + "\" \"" + tpath.FullName + "\"" + " & \"" + Directory.GetParent(tpath.FullName) + "\\StormLoader.exe\"",
                                //WindowStyle = ProcessWindowStyle.Hidden,
                                //CreateNoWindow = true,
                                FileName = "cmd.exe"
                            });
                            Environment.Exit(0);
                        }

                        LogError("An error has occured when updating a required DLL. Ensure that you have write access to the path specified in the following error, and that the file specified is not in use by anything.\n" + ex.ToString());
                        LogInfo("You may press 'D' to attempt automatic deletion of the TARGET DLL, which should allow a fresh copy on next run, or may press 'O' to open the folder that the TARGET DLL resides in.");
                        ConsoleKeyInfo k = Console.ReadKey();
                        if (k.Key == ConsoleKey.O)
                        {
                            Process.Start("explorer.exe", Directory.GetParent(tpath.FullName).ToString());
                        }
                        else if (k.Key == ConsoleKey.D)
                        {
                            Process.Start(new ProcessStartInfo()
                            {
                                Arguments = "/C choice /C Y /N /D Y /T 3 & Del \"" + tpath.FullName + "\" & Copy /V /Y \"" + spath.FullName + "\" \"" + tpath.FullName + "\"" + " & \"" + Directory.GetParent(tpath.FullName) + "\\StormLoader.exe\"",
                                //WindowStyle = ProcessWindowStyle.Hidden,
                                //CreateNoWindow = true,
                                FileName = "cmd.exe"
                            });
                            Environment.Exit(0);
                        }
                        Environment.Exit(-1);
                    }
                }
                LogInfo("DLL IS UP TO DATE");
            }
            else
            {
                LogInfo("DLL DOES NOT EXIST");
                LogInfo("CREATING...");
                try
                {
                    spath.CopyTo(tpath.FullName, true);
                }
                catch (Exception ex)
                {
                    LogError("An error has occured when updating a required DLL. Ensure that you have write access to the path specified in the following error.\n" + ex.ToString());
                    Console.ReadKey();
                    Environment.Exit(-1);
                }
                LogInfo("DLL HAS BEEN CREATED");
            }

            if (spath.LastWriteTime < tpath.LastWriteTime)
            {
                LogError("A required DLL is outdated and could not be automatically updated. Please update the TARGET DLL with the SOURCE DLL listed above.");
                Console.ReadKey();
                Environment.Exit(-2);
            }

            StardewForm = Control.FromHandle(Game.Window.Handle).FindForm();

            Command.RegisterCommand("help", "Lists all commands | 'help <cmd>' returns command description").CommandFired += help_CommandFired;
            BaseCommands.RegisterCommands();

            ConsoleThread = new Thread(RunConsole);
            ConsoleThread.Start();
        }

        [Subscribe]
        public void UpdateCallback(PreUpdateEvent @event)
        {
            BaseCommands.UpdateCallback();
        }

        [Subscribe]
        public void GameExitCallback(GameExitEvent @event)
        {
            try
            {
                ConsoleThread.Abort();
            }
            catch
            {
            }
        }

        public static void RunConsole()
        {
            AllocConsole();
            LogInfo("Storm Command Console Running");
            LogInfo("Ready for user input. Type 'help' for a list of registered commands, or 'help <cmd>' for help with a command.");

            while (true)
            {
                Command.CallCommand(Console.ReadLine());

                Thread.Sleep(1000 / 60); //Only do anything 60 times a second.
            }
        }

        public static void StardewInvoke(Action a)
        {
            StardewForm.Invoke(a);
        }

        static void help_CommandFired(object o, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                Command fnd = Command.FindCommand(e.Command.CalledArgs[0]);
                if (fnd == null)
                    LogError("The command specified could not be found");
                else
                {
                    if (fnd.CommandArgs.Length > 0)
                        LogInfo("{0}: {1} - {2}", fnd.CommandName, fnd.CommandDesc, fnd.CommandArgs.ToSingular());
                    else
                        LogInfo("{0}: {1}", fnd.CommandName, fnd.CommandDesc);
                }
            }
            else
                LogInfo("Commands: " + Command.RegisteredCommands.Select(x => x.CommandName).OrderBy(x => x).ToSingular());
        }

        public static void LoadConfig()
        {
            if (!File.Exists(ConfigLocation))
            {
                LogInfo("The config file for CommandConsole was not found, attempting creation...");
                ModConfig = new Config();
                ModConfig.AutoUpdateWithoutPrompt = true;
                ModConfig.VersionGuid = CurrentConfigGuid;
                File.WriteAllBytes(ConfigLocation, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ModConfig, Formatting.Indented)));
                LogInfo("The config file for CommandConsole has been loaded.");
                foreach (var v in ModConfig.GetType().GetProperties())
                {
                    Console.Write(v.Name + ": " + v.GetValue(ModConfig) + ", ");
                }
            }
            else
            {
                ModConfig = JsonConvert.DeserializeObject<Config>(Encoding.UTF8.GetString(File.ReadAllBytes(ConfigLocation)));

                if (ModConfig.VersionGuid != CurrentConfigGuid)
                {
                    LogInfo("The config file for CommandConsole is outdated and will be updated now.");
                    ModConfig.VersionGuid = CurrentConfigGuid;
                    File.WriteAllBytes(ConfigLocation, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ModConfig, Formatting.Indented)));
                    LogInfo("The config file for CommandConsole has been Updated.");
                }

                LogInfo("The config file for CommandConsole has been loaded.");
                foreach (var v in ModConfig.GetType().GetProperties())
                {
                    Console.Write(v.Name + ": " + v.GetValue(ModConfig) + ", ");
                }
                Console.SetCursorPosition(Console.CursorLeft - 2, Console.CursorTop);
                Console.Write("\n");

                Object o = new Object();

            }
        }


        #region Logging
        public static void Log(object o, params object[] format)
        {
            Console.WriteLine("[{0}] {1}", System.DateTime.Now, string.Format(o.ToString(), format));
        }

        public static void LogColour(ConsoleColor c, object o, params object[] format)
        {
            Console.ForegroundColor = c;
            Log(o.ToString(), format);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void LogInfo(object o, params object[] format)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Log(o, format);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void LogError(object o, params object[] format)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Log(o, format);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void LogValueNotSpecified()
        {
            LogError("<value> must be specified");
        }

        public static void LogObjectValueNotSpecified()
        {
            LogError("<object> and <value> must be specified");
        }

        public static void LogValueInvalid()
        {
            LogError("<value> is invalid");
        }

        public static void LogObjectInvalid()
        {
            LogError("<object> is invalid");
        }

        public static void LogValueNotInt32()
        {
            LogError("<value> must be a whole number (Int32)");
        }
        #endregion
    }

    public class Config
    {
        public bool AutoUpdateWithoutPrompt { get; set; }
        public string VersionGuid { get; set; }
    }
}
