using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VehicleFramework.Admin
{
    public static class VFPingManager
    {
        internal static readonly List<PingInstance> mvPings = new();
        //public static List<PingInstance> GetPingsWhere(Func<PingInstance, bool> match) => mvPings.Where(match).ToList();
        public static PingType RegisterPingType(string name, bool verbose = false)
        {
            PingType? newPingType = Nautilus.Handlers.EnumHandler.AddEntry<PingType>($"{name}PingType");
            VehicleRegistrar.VerboseLog(VehicleRegistrar.LogType.Log, verbose, $"Registering PingType for {name}.");
            return newPingType ?? throw SessionManager.Fatal(name + " PingType could not be registered!");
        }
        internal static string VFGetCachedPingTypeString(PingType inputType)
        {
            if (mvPings.Where(x => x.pingType == inputType).Any())
            {
                return mvPings.Find(x => x.pingType == inputType).name;
            }
            if (Assets.SpriteHelper.PingSprites.Where(x => x.Item2 == inputType).Any())
            {
                return Assets.SpriteHelper.PingSprites.Find(x => x.Item2 == inputType).Item1;
            }
            throw SessionManager.Fatal($"Could not find ModVehicle PingType {inputType} in VehicleManager.vehicleTypes or Assets.SpriteHelper.PingSprites!");
        }
        internal static Sprite VFGetPingTypeSprite(string name)
        {
            if (VehicleManager.GetVehicleTypesWhere(x => x.name == name).Count != 0)
            {
                return VehicleManager.GetVehicleTypesWhere(x => x.name == name).First().ping_sprite;
            }
            if (Assets.SpriteHelper.PingSprites.Where(x => x.Item1 == name).Any())
            {
                return Assets.SpriteHelper.PingSprites.Find(x => x.Item1 == name).Item3;
            }
            return Assets.StaticAssets.DefaultPingSprite
            ?? throw SessionManager.Fatal($"Could not find ModVehicle PingType {name} in VehicleManager.vehicleTypes or Assets.SpriteHelper.PingSprites, and DefaultPingSprite is null!");
        }
    }
}
