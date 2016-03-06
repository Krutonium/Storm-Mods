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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Storm.ExternalEvent;
using Storm.StardewValley;
using Storm.StardewValley.Event;
using Storm.StardewValley.Wrapper;

namespace MovementSpeedModifier
{
    [Mod(Author = "Zoryn Aaron", Name = "Movement Speed Modifier", Version = 1.0d)]
    public class MovementSpeedModifier : DiskResource
    {
        public static string ExecutionLocation { get; private set; }
        public static string ConfigLocation { get; private set; }
        public static Config ModConfig { get; private set; }

        public static Farmer Player => StaticGameContext.WrappedGame.Player;

        [Subscribe]
        public void InitializeCallback(InitializeEvent @event)
        {
            ExecutionLocation = ParentPathOnDisk;
            ConfigLocation = Path.Combine(ExecutionLocation, "Config.json");
            if (!File.Exists(ConfigLocation))
            {
                Console.WriteLine("The config file for MovementSpeedModifier was not found, attempting creation...");
                ModConfig = new Config();
                ModConfig.EnableDiagonalMovementSpeedFix = true;
                ModConfig.PlayerRunningSpeed = 5;
                ModConfig.PlayerWalkingSpeed = 2;
                File.WriteAllBytes(ConfigLocation, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ModConfig)));
                Console.WriteLine("The config file for MovementSpeedModifier has been loaded.\n\tDiagonalFix: {0}, PlayerWalkingSpeed: {1}, PlayerRunningSpeed: {2}", ModConfig.EnableDiagonalMovementSpeedFix, ModConfig.PlayerWalkingSpeed, ModConfig.PlayerRunningSpeed);
            }
            else
            {
                ModConfig = JsonConvert.DeserializeObject<Config>(Encoding.UTF8.GetString(File.ReadAllBytes(ConfigLocation)));
                Console.WriteLine("The config file for MovementSpeedModifier has been loaded.\n\tDiagonalFix: {0}, PlayerWalkingSpeed: {1}, PlayerRunningSpeed: {2}", ModConfig.EnableDiagonalMovementSpeedFix, ModConfig.PlayerWalkingSpeed, ModConfig.PlayerRunningSpeed);
            }
        }

        [Subscribe]
        public void UpdateCallback(PreUpdateEvent @event)
        {
            if (Player.Running)
                Player.Speed = ModConfig.PlayerRunningSpeed;
            else
                Player.Speed = ModConfig.PlayerWalkingSpeed;

            if (ModConfig.EnableDiagonalMovementSpeedFix)
                Player.MovementDirections.Clear();
        }
    }

    public class Config
    {
        public bool EnableDiagonalMovementSpeedFix { get; set; }
        public int PlayerWalkingSpeed { get; set; }
        public int PlayerRunningSpeed { get; set; }
    }
}
