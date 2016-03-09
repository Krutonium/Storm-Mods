using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Storm;
using Storm.ExternalEvent;
using Storm.StardewValley;
using Storm.StardewValley.Event;

namespace BetterRNG
{
    [Mod]
    public class BetterRng : DiskResource
    {
        public static MersenneTwister Twister { get; private set; }
        public static float[] RandomFloats { get; private set; }

        public static RngConfig ModConfig { get; private set; }
        public static bool JustLoadedGame { get; private set; }

        [Subscribe]
        public void InitializeCallback(InitializeEvent @event)
        {
            ModConfig = new RngConfig();
            ModConfig = (RngConfig)Config.InitializeConfig(Config.GetBasePath(this), ModConfig);
            RandomFloats = new float[1024];
            Twister = new MersenneTwister();
            
            //Just fills the buffer with junk so that we know everything is good and random.
            RandomFloats.FillFloats();

            //Determine base RNG to get everything up and running.
            DetermineRng(@event);
        }

        [Subscribe]
        public void AfterGameLoadedCallback(AfterGameLoadedEvent @event)
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
        public void AfterNewDayCallback(AfterNewDayEvent @event)
        {
            DetermineRng(@event);
        }

        //WEATHER:
        //0 = SUNNY, 1 = RAIN, 2 = CLOUDY/SNOWY, 3 = THUNDER STORM, 4 = FESTIVAL/EVENT/SUNNY, 5 = SNOW

        public static void DetermineRng(StaticContextEvent @event)
        {
            RandomFloats.FillFloats();
            @event.Root.DailyLuck = RandomFloats.Random();

            float[] weatherConfig = new[] { ModConfig.SunnyChance, ModConfig.CloudySnowyChance, ModConfig.RainyChance, ModConfig.StormyChance, ModConfig.HarshSnowyChance };
            if (weatherConfig.Where(x => x > 0).Sum() >= 0.99f && weatherConfig.Where(x => x > 0).Sum() <= 1.01f)
            {
                var floats = new[] { ProportionValue.Create(ModConfig.SunnyChance, 0), ProportionValue.Create(ModConfig.CloudySnowyChance, 2), ProportionValue.Create(ModConfig.RainyChance, 1), ProportionValue.Create(ModConfig.StormyChance, 3), ProportionValue.Create(ModConfig.HarshSnowyChance, 5) };
                @event.Root.WeatherForTomorrow = floats.ChooseByRandom();

            }
            else
                Console.WriteLine("Could not set weather because the config values do not add up to 1.0 ({0}).\n\tPlease correct this error in: " + ModConfig.ConfigLocation, "|" + weatherConfig.Where(x => x > 0).Sum());

            Console.WriteLine("Daily Luck: " + @event.Root.DailyLuck + " | Tomorrow's Weather: " + @event.Root.WeatherForTomorrow);
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
                floats[i] = BetterRng.Twister.Next(-100,100) / 1000f;
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
    }

    public class RngConfig : Config
    {
        public float SunnyChance { get; set; }
        public float CloudySnowyChance { get; set; }
        public float RainyChance { get; set; }
        public float StormyChance { get; set; }
        public float HarshSnowyChance { get; set; }

        public override Config GenerateBaseConfig(Config baseConfig)
        {
            SunnyChance = 0.60f;
            CloudySnowyChance = 0.15f;
            RainyChance = 0.15f;
            StormyChance = 0.05f;
            HarshSnowyChance = 0.05f;
            return this;
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
}
