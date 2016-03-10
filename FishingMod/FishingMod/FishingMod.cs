using System;
using Microsoft.Xna.Framework;
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
        public static StaticContext TheGame { get; private set; }
        public static Farmer Player => TheGame.Player;
        public static ClickableMenu ActiveMenu => TheGame.ActiveClickableMenu;
        public static BobberBar Bobber => TheGame.ActiveClickableMenu.ToBobberBar();

        public static bool BeganFishingGame { get; protected set; }
        public static int UpdateIndex { get; protected set; }
        public static bool HitZero { get; protected set; }

        public static FishConfig ModConfig { get; protected set; }

        [Subscribe]
        public void InitializeCallback(InitializeEvent @event)
        {
            TheGame = @event.Root;

            ModConfig = new FishConfig();
            ModConfig = (FishConfig)Config.InitializeConfig(Config.GetBasePath(this), ModConfig);

            Console.WriteLine("FishingMod by Zoryn => Initialization Completed");
        }

        [Subscribe]
        public void UpdateCallback(PostUpdateEvent @event)
        {
            if (ActiveMenu == null)
                return;

            if (ActiveMenu.IsBobberBar() && !HitZero)
            {
                //Begin fishing game
                if (!BeganFishingGame && UpdateIndex > 15)
                {
                    //Do these things once per fishing minigame, 1/4 second after it updates

                    Bobber.Difficulty *= ModConfig.FishDifficultyMultiplier;
                    Bobber.Difficulty += ModConfig.FishDifficultyAdditive;

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

                if (ModConfig.AlwaysPerfect)
                    Bobber.Perfect = true;

                if (Bobber.DistanceFromCatching <= 0.1f)
                    HitZero = true;

                if (!Bobber.BobberInBar)
                    Bobber.DistanceFromCatching += ModConfig.CatchPerTickAddition;
            }
            else
            {
                //End fishing game
                BeganFishingGame = false;
                UpdateIndex = 0;
                HitZero = false;
            }
        }

        [Subscribe]
        public void DoneFishing(PreDoneFishingEvent @event)
        {
            if (!ModConfig.StaminaLossMultiplier.Equals(1))
            {
                //[8 - (@event.LocalPlayer.FishingLevel * 0.1f)] is the hardcoded function in tool use for fishing
                float loss = (8 - (@event.LocalPlayer.FishingLevel * 0.1f));
                @event.LocalPlayer.Stamina += loss;
                @event.LocalPlayer.Stamina -= loss * ModConfig.StaminaLossMultiplier;
            }

            if (@event.ConsumeBaitAndTackle)
            {
                if (ModConfig.BaitTackleSettingsApplyOnlyToTrash)
                    return;
                RegenBait(@event.Rod);
                RegenTackle(@event.Rod);
            }
        }

        [Subscribe]
        public void BeforePullFishFromWaterCallback(PrePullFishFromWaterEvent @event)
        {
            FishingRod f = @event.Rod;

            if (ModConfig.BaitTackleSettingsApplyOnlyToTrash)
            {
                if (@event.FishDifficulty == 0 && @event.FishQuality == 0 && @event.FishSize == -1)
                {
                    //Fished up trash
                    RegenBait(f);
                    RegenTackle(f);
                }
            }
            else
            {
                RegenBait(f);
                RegenTackle(f);
            }
        }

        void RegenBait(FishingRod f)
        {
            if (f.Attachments[0] != null)
            {
                if (ModConfig.InfiniteBait)
                    f.Attachments[0].Stack += 1;
            }
        }

        void RegenTackle(FishingRod f)
        {
            if (f.Attachments[1] != null)
            {
                if (f.Attachments[1].Expose() != null)
                {
                    if (ModConfig.InfiniteTackle)
                    {
                        f.Attachments[1].Scale += new Vector2(0, 0.05f);
                        return;
                    }

                    f.Attachments[1].Scale += new Vector2(0, ModConfig.TackleDurabilityAdditive);

                    f.Attachments[1].Scale *= new Vector2(1, ModConfig.TackleDurabilityMultiplier);

                    f.Attachments[1].Scale = new Vector2(f.Attachments[1].Scale.X.Clamp(0, 1), f.Attachments[1].Scale.Y.Clamp(0, 1));
                }
            }
        }

        [Subscribe]
        public void ChatMessageEnteredCallback(ChatMessageEnteredEvent @event)
        {
            Command c = Command.ParseCommand(@event.ChatText);
            if (c.Name == "rlcfg" && c.HasArgs && (c.Args[0] == "fishingmod" || c.Args[0] == "all"))
            {
                Console.WriteLine("Reloading the config for FishingMod by Zoryn");
                ModConfig = new FishConfig();
                ModConfig = (FishConfig)Config.InitializeConfig(Config.GetBasePath(this), ModConfig);
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
        public float FishDifficultyAdditive { get; set; }
        public float CatchPerTickAddition { get; set; }
        public float StaminaLossMultiplier { get; set; }

        public bool BaitTackleSettingsApplyOnlyToTrash { get; set; }
        public bool InfiniteTackle { get; set; }
        public bool InfiniteBait { get; set; }
        public float TackleDurabilityAdditive { get; set; }
        public float TackleDurabilityMultiplier { get; set; }

        public override Config GenerateBaseConfig(Config baseConfig)
        {
            AlwaysPerfect = false;
            AlwaysFindTreasure = false;
            InstantCatchFish = false;
            InstantCatchTreasure = false;
            CatchPerTickAddition = 2 / 1000f;
            StaminaLossMultiplier = 0.5f;

            FishDifficultyMultiplier = 1;
            FishDifficultyAdditive = 0;
            BaitTackleSettingsApplyOnlyToTrash = true;
            InfiniteTackle = true;
            InfiniteBait = true;
            TackleDurabilityMultiplier = 1;
            TackleDurabilityAdditive = 0;
            return this;
        }
    }

    public static class Extensions
    {
        public static float Clamp(this float value, float min, float max)
        {
            if (value <= min) { return min; }
            if (value >= max) { return max; }
            return value;
        }
    }
}
