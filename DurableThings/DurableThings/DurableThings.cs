using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Storm;
using Storm.ExternalEvent;
using Storm.StardewValley;
using Storm.StardewValley.Accessor;
using Storm.StardewValley.Event;
using Storm.StardewValley.Event.FishingRod;
using Storm.StardewValley.Proxy;
using Storm.StardewValley.Wrapper;

namespace DurableThings
{
    [Mod]
    public sealed class DurableThings : DiskResource
    {
        public DurableConfig ModConfig { get; private set; }

        [Subscribe]
        public void InitializeCallback(InitializeEvent @event)
        {
            ModConfig = new DurableConfig();
            ModConfig = (DurableConfig)Config.InitializeConfig(Config.GetBasePath(this), ModConfig);

            Console.WriteLine("DurableThings by Zoryn => Initialization Completed");
        }

        #region Fishing Durabililty Things

        public void DoneFishing(BeforeDoneFishingEvent @event)
        {
            if (@event.ConsumeBaitAndTackle)
            {
                if (ModConfig.OnlyAppliesToTrash)
                    return;
                RegenBait(@event.Rod);
                RegenTackle(@event.Rod);
            }
        }
             
        [Subscribe]
        public void BeforePullFishFromWaterCallback(BeforePullFishFromWaterEvent @event)
        {
            FishingRod f = @event.Rod;

            if (ModConfig.OnlyAppliesToTrash)
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

        #endregion
    }

    public class DurableConfig : Config
    {
        public bool DurableTackle { get; set; }
        public bool OnlyAppliesToTrash { get; set; }
        public bool InfiniteTackle { get; set; }
        public bool InfiniteBait { get; set; }
        public float TackleDurabilityAdditive { get; set; }
        public float TackleDurabilityMultiplier { get; set; }

        public override Config GenerateBaseConfig(Config baseConfig)
        {
            DurableTackle = false;
            OnlyAppliesToTrash = true;
            InfiniteTackle = true;
            InfiniteBait = true;
            TackleDurabilityAdditive = 0;
            TackleDurabilityMultiplier = 1;
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
