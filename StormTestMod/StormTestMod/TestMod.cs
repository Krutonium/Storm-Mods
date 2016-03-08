using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Storm;
using Storm.ExternalEvent;
using Storm.StardewValley;
using Storm.StardewValley.Accessor;
using Storm.StardewValley.Event;
using Storm.StardewValley.Event.Game;
using Storm.StardewValley.Wrapper;

namespace StormTestMod
{
    [Mod]
    public class TestMod : DiskResource
    {
        public static TestConfig TConfig { get; private set; }
        [Subscribe]
        public void InitializeCallback(InitializeEvent @event)
        {
            TConfig = new TestConfig(); //THIS IS REQUIRED

            //Initializes the config class, loading an existing config or generating a new one.
            //GetBasePath will return a string to the DLL's folder plus "Config.json", so that the config file is next to the DLL.
            //This should be used unless a modder specifically wants to initialize a config in anoter location.
            TConfig = (TestConfig)Config.InitializeConfig(Config.GetBasePath(this), TConfig);

            //BASE JSON OUTPUT AFTER RUNNING THE FIRST TIME:
            /*
                {
                  "AnExampleField": "DefaultValue",
                  "WillIDoSomething": false,
                  "MoveSpeedOrSomething": 5
                }
            */
            //SECOND RUN - NOTHING CHANGED - THE FILE IS ONLY UPDATED IF THINGS ARE ADDED/REMOVED IN THE CLASS (OR THE FILE IS INVALID)
            /*
                {
                  "AnExampleField": "DefaultValue",
                  "WillIDoSomething": false,
                  "MoveSpeedOrSomething": 5,
                }
            */
            //NOW A USER CHANGES THE BOOL TO TRUE AND RUNS IT AGAIN. OUTPUT:
            /*
                {
                  "AnExampleField": "DefaultValue",
                  "WillIDoSomething": true, //ONLY THIS CHANGED, BECAUSE OF THE USER - THE FILE RETAINS ITS STATE
                  "MoveSpeedOrSomething": 5,
                }
            */

            //If an invalid value appears in any field, the JSON file will be renamed and the user will be notified.
            //If a value is removed from the inherited config class, it will be stripped from the JSON on the next load.
            //If a value is added to the inherited config class, it will be added to the JSON on the next load.
        }
    }

    public class TestConfig : Config
    {
        //BEGIN FIELDS IN FIRST RUN
        public string AnExampleField { get; set; }
        public bool WillIDoSomething { get; set; }
        public int MoveSpeedOrSomething { get; set; }
        //END FIRST RUN

        public override Config GenerateBaseConfig(Config baseConfig)
        {
            //FIRST RUN
            AnExampleField = "DefaultValue";
            WillIDoSomething = false;
            MoveSpeedOrSomething = 5;
            //END

            return this;
        }
    }
}
