using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Storm;
using Storm.ExternalEvent;
using Storm.StardewValley;
using Storm.StardewValley.Accessor;
using Storm.StardewValley.Event;
using Storm.StardewValley.Wrapper;

namespace FishingMod
{
    [Mod]
    public class LessFishingLoss : DiskResource
    {
        public ClickableMenu ActiveMenu => StaticGameContext.WrappedGame.ActiveClickableMenu;

        public BobberBarAccessor BobberAcc => (BobberBarAccessor)StaticGameContext.WrappedGame.ActiveClickableMenu.Expose();

        public BobberBar Bobber => new BobberBar(StaticGameContext.WrappedGame, BobberAcc);

        public static bool BeganFishingGame { get; protected set; }
        public static int UpdateIndex { get; protected set; }

        public static Farmer Player => StaticGameContext.WrappedGame.Player;

        public static FishConfig ModConfig { get; protected set; }

        [Subscribe]
        public void InitializeCallback(InitializeEvent @event)
        {
            ModConfig = new FishConfig();
            ModConfig = (FishConfig)Config.InitializeConfig(Config.GetBasePath(this), ModConfig);

            Console.WriteLine("FishingMod by Zoryn => Initialization Completed");
        }

        [Subscribe]
        public void UpdateCallback(PostUpdateEvent @event)
        {
            if (ActiveMenu == null)
                return;

            if (ActiveMenu.Expose() is BobberBarAccessor)
            {
                //Begin fishing game
                if (!BeganFishingGame && UpdateIndex > 15)
                {
                    //Do these things once per fishing minigame, 1/4 second after it updates
                    Console.WriteLine("WINNING NOW");

                    Bobber.Difficulty *= ModConfig.FishDifficultyMultiplier;
                    Bobber.Difficulty += ModConfig.FishDifficultyAdder;

                    if (ModConfig.AlwaysFindTreasure)
                        Bobber.Treasure = true;

                    if (ModConfig.InstantCatchFish)
                        Bobber.DistanceFromCatching += 100;

                    if (ModConfig.InstantCatchTreasure)
                        if (Bobber.Treasure || ModConfig.AlwaysFindTreasure)
                            Bobber.TreasureCaught = true;

                    BeganFishingGame = true;
                }

                if (UpdateIndex < 20)
                    UpdateIndex++;

                Console.Write(UpdateIndex + " > ");

                if (ModConfig.AlwaysPerfect)
                    Bobber.Perfect = true;

                if (!Bobber.BobberInBar)
                    Bobber.DistanceFromCatching += ModConfig.CatchPerTickAddition;
            }
            else
            {
                //End fishing game
                BeganFishingGame = false;
                UpdateIndex = 0;
            }
        }
    }

    public class FishConfig : Config
    {
        public bool AlwaysPerfect { get; set; }
        public bool AlwaysFindTreasure { get; set; }
        public bool InstantCatchFish { get; set; }
        public bool InstantCatchTreasure { get; set; }
        public float FishDifficultyMultiplier { get; set; }
        public float FishDifficultyAdder { get; set; }
        public float CatchPerTickAddition { get; set; }

        public override Config GenerateBaseConfig(Config baseConfig)
        {
            AlwaysPerfect = false;
            AlwaysFindTreasure = false;
            InstantCatchFish = false;
            InstantCatchTreasure = false;
            CatchPerTickAddition = 2 / 1000f;
            FishDifficultyMultiplier = 1;
            FishDifficultyAdder = 0;
            return this;
        }
    }
}
