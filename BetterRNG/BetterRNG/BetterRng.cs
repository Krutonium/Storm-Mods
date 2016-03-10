using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using Microsoft.Xna.Framework.Input;
using Storm;
using Storm.ExternalEvent;
using Storm.StardewValley;
using Storm.StardewValley.Accessor;
using Storm.StardewValley.Event;
using Storm.StardewValley.Wrapper;

namespace BetterRNG
{
    [Mod]
    public class BetterRng : DiskResource
    {
        public static MersenneTwister Twister { get; private set; }
        public static float[] RandomFloats { get; private set; }

        public static RngConfig ModConfig { get; private set; }
        public static bool JustLoadedGame { get; private set; }

        #region Fishing

        public static bool BeganFishingGame { get; protected set; }
        public static int UpdateIndex { get; protected set; }
        public static bool HitZero { get; protected set; }
        public IEnumerable<ProportionValue<Int32>> OneToThree { get; protected set; }
        public IEnumerable<ProportionValue<Int32>> OneToFive { get; protected set; }
        public IEnumerable<ProportionValue<Int32>> OneToTen { get; protected set; }

        #endregion

        [Subscribe]
        public void InitializeCallback(InitializeEvent @event)
        {
            ModConfig = new RngConfig();
            ModConfig = (RngConfig)Config.InitializeConfig(Config.GetBasePath(this), ModConfig);
            RandomFloats = new float[256];
            Twister = new MersenneTwister();
            
            //Destroys the game's built-in random number generator for Twister.
            @event.Root.Random = Twister;

            //Just fills the buffer with junk so that we know everything is good and random.
            RandomFloats.FillFloats();

            //Determine base RNG to get everything up and running.
            DetermineRng(@event);

            OneToThree = new[] {ProportionValue.Create(80, 1), ProportionValue.Create(15, 2), ProportionValue.Create(5, 3)};
            OneToFive = new[] {ProportionValue.Create(80, 1), ProportionValue.Create(15, 2), ProportionValue.Create(3, 3), ProportionValue.Create(1.9f, 4), ProportionValue.Create(0.1f, 5)};
            OneToTen = new[] { ProportionValue.Create(80, 1), ProportionValue.Create(13, 2), ProportionValue.Create(3, 3), ProportionValue.Create(2, 4), ProportionValue.Create(1, 5),
                ProportionValue.Create(0.5f, 6), ProportionValue.Create(0.25f, 7), ProportionValue.Create(0.15f, 8), ProportionValue.Create(0.09f, 9), ProportionValue.Create(0.01f, 10)};
            
            Console.WriteLine("BetterRng by Zoryn => Initialization Completed");
        }

        #region Daily RNG

        [Subscribe]
        public void AfterGameLoadedCallback(PostGameLoadedEvent @event)
        {
            JustLoadedGame = true;
        }

        [Subscribe]
        public void PlayMorningSongCallback(PlayMorningSongEvent @event)
        {
            //Loading is async for some reason... so we'll just keep track that we initiated loading and then when this event fires trigger the rng manipulation
            if (JustLoadedGame)
            {
                DetermineRng(@event);
                JustLoadedGame = false;
            }
        }

        [Subscribe]
        public void AfterNewDayCallback(PostNewDayEvent @event)
        {
            DetermineRng(@event);
        }

        public static void DetermineRng(StaticContextEvent @event)
        {
            //0 = SUNNY, 1 = RAIN, 2 = CLOUDY/SNOWY, 3 = THUNDER STORM, 4 = FESTIVAL/EVENT/SUNNY, 5 = SNOW
            //Generate a good set of new random numbers to choose from for daily luck every morning.
            RandomFloats.FillFloats();
            @event.Root.DailyLuck = RandomFloats.Random() / 10;

            float[] weatherConfig = new[] { ModConfig.SunnyChance, ModConfig.CloudySnowyChance, ModConfig.RainyChance, ModConfig.StormyChance, ModConfig.HarshSnowyChance };
            if (weatherConfig.Sum() >= 0.99f && weatherConfig.Sum() <= 1.01f)
            {
                var floats = new[] { ProportionValue.Create(ModConfig.SunnyChance, 0), ProportionValue.Create(ModConfig.CloudySnowyChance, 2), ProportionValue.Create(ModConfig.RainyChance, 1), ProportionValue.Create(ModConfig.StormyChance, 3), ProportionValue.Create(ModConfig.HarshSnowyChance, 5) };
                int targWeather = floats.ChooseByRandom();
                if (targWeather == 5 && @event.Root.CurrentSeason != "winter")
                    targWeather = 3;
                @event.Root.WeatherForTomorrow = targWeather;
            }
            else
                Console.WriteLine("Could not set weather because the config values do not add up to 1.0 ({0}).\n\tPlease correct this error in: " + ModConfig.ConfigLocation, weatherConfig.Sum());

            //Console.WriteLine("[Twister] Daily Luck: " + @event.Root.DailyLuck + " | Tomorrow's Weather: " + @event.Root.WeatherForTomorrow);
        }

        #endregion

        #region FishingRng

       
        [Subscribe]
        public void PreUpdateCallback(PreUpdateEvent @event)
        {
            if (!ModConfig.EnableFishingTreasureOverride && !ModConfig.EnableFishingStuffOverride)
                return;

            if (@event.Root.ActiveClickableMenu == null)
                return;

            if (@event.Root.ActiveClickableMenu.IsBobberBar() && !HitZero)
            {
                BobberBar Bobber = @event.Root.ActiveClickableMenu.ToBobberBar();
                //Begin fishing game
                if (!BeganFishingGame && UpdateIndex > 20)
                {
                    Console.WriteLine("BEGIN FISHING");
                    //Do these things once per fishing minigame, 1/3 second after it updates
                    //This will override anything from the FishingMod by me, and from any other mod that modifies these things before this

                    if (ModConfig.EnableFishingTreasureOverride)
                        @event.Root.ActiveClickableMenu.ToBobberBar().Treasure = Twister.NextBool();

                    if (ModConfig.EnableFishingStuffOverride)
                    {
                        float baseDiff = Bobber.Difficulty;
                        Bobber.Difficulty *= (OneToFive.ChooseByRandom() + RandomFloats.Random().Abs());
                        float diffDiff = Bobber.Difficulty / baseDiff;
                        Console.WriteLine("DIFFICULTY DIFFERENCE: " + diffDiff);

                        Console.WriteLine("PRE MIN/MAX: {0}/{1}", Bobber.MinFishSize, Bobber.MaxFishSize);
                        Bobber.MinFishSize = (int)Math.Round(Bobber.MinFishSize * RandomFloats.Random().Abs());
                        Bobber.MaxFishSize = (int)Math.Round(Bobber.MaxFishSize * (OneToFive.ChooseByRandom() + RandomFloats.Random().Abs()));
                        Console.WriteLine("MIN/MAX: {0}/{1}", Bobber.MinFishSize, Bobber.MaxFishSize);
                        Bobber.FishSize = Twister.Next(Bobber.MinFishSize, Bobber.MaxFishSize);
                        Console.WriteLine("SET SIZE TO: " + Bobber.FishSize);
                        if (@event.EventBus.mods.Exists(x => x.Author == "Zoryn" && x.Name == "Quality Extender"))
                            @event.Root.ActiveClickableMenu.ToBobberBar().FishQuality = OneToTen.ChooseByRandom();
                        else
                            @event.Root.ActiveClickableMenu.ToBobberBar().FishQuality = OneToThree.ChooseByRandom() - 1;
                    }

                    BeganFishingGame = true;
                }

                if (UpdateIndex % 60 == 0)
                    Console.WriteLine(UpdateIndex + " - "  + @event.Root.ActiveClickableMenu.ToBobberBar().FishSize);

                UpdateIndex++;

                if (@event.Root.ActiveClickableMenu.ToBobberBar().DistanceFromCatching <= 0.05f)
                    HitZero = true;
            }
        }

        //Out-of-context example of pressing E will catch a fish
        [Subscribe]
        public void KeyPressCallback(KeyPressedEvent @event)
        {
            if (BeganFishingGame)
            {
                if (@event.Key == Keys.E)
                    @event.Root.ActiveClickableMenu.ToBobberBar().DistanceFromCatching = 10;
            }
        }

        [Subscribe]
        public void PostDoneFishingCallback(PostDoneFishingEvent @event)
        {
            Console.WriteLine("END FISHING");
            //End fishing game
            BeganFishingGame = false;
            UpdateIndex = 0;
            HitZero = false;
        }

        #endregion

        [Subscribe]
        public void ChatMessageEnteredCallback(ChatMessageEnteredEvent @event)
        {
            Command c = Command.ParseCommand(@event.ChatText);
            if (c.Name == "rlcfg" && c.HasArgs && (c.Args[0] == "betterrng" || c.Args[0] == "all"))
            {
                Console.WriteLine("Reloading the config for BetterRNG by Zoryn");
                ModConfig = new RngConfig();
                ModConfig = (RngConfig)Config.InitializeConfig(Config.GetBasePath(this), ModConfig);
            }
        }

        [Subscribe]
        public void a(PreHoeDirtCanPlantEvent @event)
        {
            Console.WriteLine("[{0}] CROP: {1} - NULL: {2}", System.DateTime.Now, @event.HoeDirt?.Crop, @event.HoeDirt?.Crop == null);
        }

        public static StaticContext GetGame(StaticContextEvent @event)
        {
            return @event.Root;
        }
    }

    public class RngConfig : Config
    {
        public bool EnableDailyLuckOverride { get; set; }
        public bool EnableWeatherOverride { get; set; }
        public float SunnyChance { get; set; }
        public float CloudySnowyChance { get; set; }
        public float RainyChance { get; set; }
        public float StormyChance { get; set; }
        public float HarshSnowyChance { get; set; }

        public bool EnableFishingTreasureOverride { get; set; }
        public float FishingTreasureChance { get; set; }
        public bool EnableFishingStuffOverride { get; set; }

        public override Config GenerateBaseConfig(Config baseConfig)
        {
            EnableDailyLuckOverride = true;
            EnableWeatherOverride = true;
            SunnyChance = 0.60f;
            CloudySnowyChance = 0.15f;
            RainyChance = 0.15f;
            StormyChance = 0.05f;
            HarshSnowyChance = 0.05f;

            EnableFishingTreasureOverride = true;
            FishingTreasureChance = 1 / 16f;
            EnableFishingStuffOverride = true;
            return this;
        }
    }


    public static class Extensions
    {
        public static float[] DynamicDowncast(this Byte[] bytes)
        {
            float[] f = new float[bytes.Length / 4];
            for (int i = 0; i < f.Length; i++)
            {
                f[i] = BitConverter.ToSingle(bytes, i == 0 ? 0 : (i * 4) - 1);
            }
            return f;
        }

        public static void FillFloats(this float[] floats)
        {
            for (int i = 0; i < floats.Length; i++)
                floats[i] = BetterRng.Twister.Next(-100,100) / 100f;
        }

        public static T Random<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException(nameof(enumerable));
            }

            var list = enumerable as IList<T> ?? enumerable.ToList();
            return list.Count == 0 ? default(T) : list[BetterRng.Twister.Next(0, list.Count)];
        }

        public static float Abs(this float f)
        {
            return Math.Abs(f);
        }

        public static T ChooseByRandom<T>(this IEnumerable<ProportionValue<T>> collection)
        {
            var rnd = BetterRng.Twister.NextDouble();
            foreach (var item in collection)
            {
                if (rnd < item.Proportion)
                    return item.Value;
                rnd -= item.Proportion;
            }
            throw new InvalidOperationException("The proportions in the collection do not add up to 1.");
        }
    }

    public class ProportionValue<T>
    {
        public double Proportion { get; set; }
        public T Value { get; set; }

        
    }

    public static class ProportionValue
    {
        public static ProportionValue<T> Create<T>(double proportion, T value)
        {
            return new ProportionValue<T> { Proportion = proportion, Value = value };
        }
    }
}
