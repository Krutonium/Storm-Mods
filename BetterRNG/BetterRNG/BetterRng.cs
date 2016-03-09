using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        [Subscribe]
        public void InitializeCallback(InitializeEvent @event)
        {
            ModConfig = new RngConfig();
            Config.InitializeConfig(Config.GetBasePath(this), ModConfig);
            RandomFloats = new float[1024];
            Twister = new MersenneTwister();
            
            //Just fills the buffer with junk so that we know everything is good and random.
            RandomFloats.FillFloats(); 
        }

        [Subscribe]
        public void AfterGameLoadedCallback(AfterGameLoadedEvent @event)
        {
            DetermineRng(@event);
        }

        [Subscribe]
        public void AfterNewDayCallback(AfterNewDayEvent @event)
        {
            DetermineRng(@event);
        }

        [Subscribe]
        public void a(After10MinuteClockUpdateEvent @event)
        {
            Console.WriteLine(@event.Root.DailyLuck);
        }

        //WEATHER:
        //0 = SUNNY, 1 = RAIN, 2 = CLOUDY/SNOWY, 3 = THUNDER STORM, 4 = FESTIVAL/EVENT/SUNNY, 5 = SNOW

        public static void DetermineRng(StaticContextEvent @event)
        {
            RandomFloats.FillFloats();
            @event.Root.DailyLuck = RandomFloats.Random();

            //MUST GO: SUNNY, CLOUDY, RAINY, STORMY, SNOWY
            float[] weatherConfig = new[] { ModConfig.SunnyChance, ModConfig.CloudyChance, ModConfig.RainyChance, ModConfig.StormyChance, ModConfig.SnowyChance };
            if (weatherConfig.Sum() >= 0.99f && weatherConfig.Sum() <= 1.01f)
            {
                @event.Root.WeatherForTomorrow = RandomFloats.Random().GetWeatherFromFloat(weatherConfig);
            }
            else
                Console.WriteLine("Could not set weather because the config values do not add up to 1.0 (100%).\n\tPlease correct this error in: " + ModConfig.ConfigLocation);

            Console.WriteLine("Daily Luck: " + @event.Root.Expose()._GetDailyLuck());

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

        public static int GetWeatherFromFloat(this float ff, float[] weatherConditions)
        {
            //RETURNS: 0 = SUNNY, 1 = RAIN, 2 = CLOUDY/SNOWY, 3 = THUNDER STORM, 4 = FESTIVAL/EVENT/SUNNY, 5 = SNOW
            //ORDER: SUNNY, CLOUDY, RAINY, STORMY, SNOWY

            float f = Math.Abs(ff);



            return 0;
        }
    }

    public class RngConfig : Config
    {
        public float SunnyChance { get; set; }
        public float CloudyChance { get; set; }
        public float RainyChance { get; set; }
        public float StormyChance { get; set; }
        public float SnowyChance { get; set; }

        public override Config GenerateBaseConfig(Config baseConfig)
        {
            SunnyChance = 0.55f;
            CloudyChance = 0.20f;
            RainyChance = 0.10f;
            StormyChance = 0.05f;
            SnowyChance = 0.10f;
            return this;
        }
    }
}
