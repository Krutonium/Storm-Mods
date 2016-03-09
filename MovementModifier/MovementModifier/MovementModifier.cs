/*
    Copyright 2016 Zoey (Zoryn)

    Storm is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Storm is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Storm.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using Storm;
using Storm.ExternalEvent;
using Storm.StardewValley;
using Storm.StardewValley.Event;
using Storm.StardewValley.Wrapper;

namespace MovementModifier
{
    [Mod]
    public class MovementModifier : DiskResource
    {
        public static string ExecutionLocation { get; private set; }
        public static string ConfigLocation { get; private set; }
        public static MovementConfig ModConfig { get; private set; }

        public static Farmer Player => StaticGameContext.WrappedGame.Player;

        [Subscribe]
        public void InitializeCallback(InitializeEvent @event)
        {
            ModConfig = new MovementConfig();
            ModConfig = (MovementConfig) Config.InitializeConfig(Config.GetBasePath(this), ModConfig);

            Console.WriteLine("MovementModifier by Zoryn => Initialization Completed");
        }

        [Subscribe]
        public void UpdateCallback(PreUpdateEvent @event)
        {
            if (@event.Root.CurrentLocation?.CurrentEvent?.Expose() != null)
            {
                Player.AddedSpeed = 0;
            }
            else
            {
                if (ModConfig.EnableRunningSpeedOverride && Player.Running)
                    Player.AddedSpeed = ModConfig.PlayerRunningSpeed;
                else if (ModConfig.EnableWalkingSpeedOverride && !Player.Running)
                    Player.AddedSpeed = ModConfig.PlayerWalkingSpeed;

                if (ModConfig.EnableDiagonalMovementSpeedFix)
                    Player.MovementDirections.Clear();
            }
        }

        [Subscribe]
        public void FarmerCollideWithCallback(FarmerCollideWithEvent @event)
        {
            if (ModConfig.NoClip)
            {
                @event.ReturnValue = false;
                @event.ReturnEarly = true;
            }
        }

        [Subscribe]
        public void ShouldCollideWithBuildingLayerCallback(ShouldCollideWithBuildingLayerEvent @event)
        {
            if (ModConfig.NoClip)
            {
                @event.ReturnValue = false;
                @event.ReturnEarly = true;
            }
        }

        [Subscribe]
        public void ChatMessageEnteredCallback(ChatMessageEnteredEvent @event)
        {
            Command c = Command.ParseCommand(@event.ChatText);
            if (c.Name == "rlcfg" && c.HasArgs && c.Args[0] == "movementmod")
            {
                Console.WriteLine("Reloading the config for MovementMod by Zoryn");
                ModConfig = new MovementConfig();
                ModConfig = (MovementConfig)Config.InitializeConfig(Config.GetBasePath(this), ModConfig);

                @event.ReturnValue = null;
                @event.ReturnEarly = true;
            }
        }
    }

    public class MovementConfig : Config
    {
        public bool EnableDiagonalMovementSpeedFix { get; set; }
        public bool NoClip { get; set; }
        public bool EnableWalkingSpeedOverride { get; set; }
        public int PlayerWalkingSpeed { get; set; }
        public bool EnableRunningSpeedOverride { get; set; }
        public int PlayerRunningSpeed { get; set; }

        public override Config GenerateBaseConfig(Config baseConfig)
        {
            EnableDiagonalMovementSpeedFix = true;
            NoClip = false;
            EnableWalkingSpeedOverride = false;
            PlayerWalkingSpeed = 2;
            EnableRunningSpeedOverride = false;
            PlayerRunningSpeed = 5;
            return this;
        }
    }
}