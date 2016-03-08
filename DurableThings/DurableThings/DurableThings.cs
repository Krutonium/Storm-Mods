using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Storm;
using Storm.ExternalEvent;
using Storm.StardewValley.Event;
using Storm.StardewValley.Event.FishingRod;
using Storm.StardewValley.Wrapper;

namespace DurableThings
{
    [Mod]
    public sealed class DurableThings : DiskResource
    {
        public DurableConfig ModConfig { get; protected set; }
        [Subscribe]
        public void InitializeCallback(InitializeEvent @event)
        {
            ModConfig = new DurableConfig();
            ModConfig = (DurableConfig)Config.InitializeConfig(Config.GetBasePath(this), ModConfig);
        }

        [Subscribe]
        public void BeforeDoneFishingCallback(BeforeDoneFishingEvent @event)
        {
            if (ModConfig.DurableTackle)
            {
                Tool t = @event.Who.ToolBox.ElementAtOrDefault(@event.Who.CurrentToolIndex);
                if (t is FishingRod)
                {
                    FishingRod f = (FishingRod)t;
                    //f.Attachments[0] is bait
                    //f.Attachments[1] is tackle
                    if (f.Attachments[0] != null)
                    {
                        if (ModConfig.InfiniteBait)
                            f.Attachments[0].Stack = 500;
                    }

                    if (f.Attachments[1] != null)
                    {
                        if (ModConfig.InfiniteTackle)
                            f.Attachments[1].Scale += new Vector2(f.Attachments[1].Scale.X, 1);

                        f.Attachments[1].Scale += new Vector2(0, ModConfig.TackleDurabilityAdditive);

                        f.Attachments[1].Scale *= new Vector2(1, ModConfig.TackleDurabilityMultiplier);
                    }
                }
            }
        }
    }

    public class DurableConfig : Config
    {
        public bool DurableTackle { get; set; }
        public bool InfiniteTackle { get; set; }
        public bool InfiniteBait { get; set; }
        public float TackleDurabilityAdditive { get; set; }
        public float TackleDurabilityMultiplier { get; set; }

        public override Config GenerateBaseConfig(Config baseConfig)
        {
            DurableTackle = false;
            InfiniteTackle = false;
            InfiniteBait = false;
            TackleDurabilityAdditive = 0;
            TackleDurabilityMultiplier = 1;
            return this;
        }
    }
}
