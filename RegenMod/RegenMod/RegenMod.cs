using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storm;
using Storm.ExternalEvent;
using Storm.StardewValley.Event;

namespace RegenMod
{
    [Mod]
    public class RegenMod : DiskResource
    {
        public static RegenConfig ModConfig { get; private set; }
        public static float HealthFloat { get; private set; }
        public static float StaminaFloat { get; private set; }

        [Subscribe]
        public void InitializeCallback(InitializeEvent @event)
        {
            ModConfig = new RegenConfig();
            ModConfig = (RegenConfig)Config.InitializeConfig(Config.GetBasePath(this), ModConfig);

            Console.WriteLine("RegenMod by Zoryn => Initialization Completed");
        }

        [Subscribe]
        public void UpdateCallback(PreUpdateEvent @event)
        {
            if (@event.LocalPlayer.Expose()?._GetName() == string.Empty)
                return;

            if (ModConfig.RegenHealth)
            {
                if (!ModConfig.RegenHealthOnlyWhileStill || (ModConfig.RegenHealthOnlyWhileStill && @event.LocalPlayer.TimerSinceLastMovement > 1000))
                {
                    HealthFloat += ModConfig.RegenHealthPerSecond * (float) (@event.Root.CurrentGameTime.ElapsedGameTime.TotalMilliseconds / 1000);

                    if (HealthFloat > 1)
                    {
                        @event.LocalPlayer.Health += 1;
                        HealthFloat -= 1;
                    }
                }
            }
            
            if (ModConfig.RegenStamina)
            {
                if (!ModConfig.RegenStaminaOnlyWhileStill || (ModConfig.RegenStaminaOnlyWhileStill && @event.LocalPlayer.TimerSinceLastMovement > 1000))
                {
                    StaminaFloat += ModConfig.RegenStaminaPerSecond * (float) (@event.Root.CurrentGameTime.ElapsedGameTime.TotalMilliseconds / 1000);

                    if (StaminaFloat > 1)
                    {
                        @event.LocalPlayer.Stamina += 1;
                        StaminaFloat -= 1;
                    }
                }
            }
        }

        [Subscribe]
        public void ChatMessageEnteredCallback(ChatMessageEnteredEvent @event)
        {
            Command c = Command.ParseCommand(@event.ChatText);
            if (c.Name == "rlcfg" && c.HasArgs && c.Args[0] == "regenmod")
            {
                Console.WriteLine("Reloading the config for RegenMod by Zoryn");
                ModConfig = new RegenConfig();
                ModConfig = (RegenConfig)Config.InitializeConfig(Config.GetBasePath(this), ModConfig);

                @event.ReturnValue = null;
                @event.ReturnEarly = true;
            }
        }
    }

    public class RegenConfig : Config
    {
        public bool RegenStamina { get; set; }
        public bool RegenStaminaOnlyWhileStill { get; set; }
        public float RegenStaminaPerSecond { get; set; }

        public bool RegenHealth { get; set; }
        public bool RegenHealthOnlyWhileStill { get; set; }
        public float RegenHealthPerSecond { get; set; }

        public override Config GenerateBaseConfig(Config baseConfig)
        {
            RegenStamina = true;
            RegenStaminaOnlyWhileStill = true;
            RegenStaminaPerSecond = 0.5f;

            RegenHealth = true;
            RegenHealthOnlyWhileStill = true;
            RegenHealthPerSecond = 0.25f;

            return this;
        }
    }
}
