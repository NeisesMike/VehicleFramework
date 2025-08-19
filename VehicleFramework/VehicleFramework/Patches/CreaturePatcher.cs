using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using VehicleFramework.VehicleTypes;
using VehicleFramework.Interfaces;

// PURPOSE: ModVehicles have an awareness of nearby leviathans
// VALUE: Moderate. Allows for some very cool/scary moments.

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(Creature))]
    [HarmonyPatch(nameof(Creature.ChooseBestAction))]
    class CreaturePatcher
    {
        private static readonly List<string> LeviathanNames = new()
        {
            "GhostLeviathan",
            "ReaperLeviathan",
            "SeaDragon",
            "BlazaLeviathan",
            "AbyssalBlaza",
            "Bloop",
            "DeepBloop",
            "GrandBloop",
            "AncientBloop",
            "GulperLeviathan",
            "GulperLeviathanJuvenile",
            "GargantuanVoid",
            "GargantuanJuvenile",
            "AbyssalBlaza",
            "AnglerFish",
        };

        [HarmonyPostfix]
        public static void Postfix(Creature __instance)
        {
            if (Player.main.GetVehicle() is not ModVehicle mv) return;

            if (Vector3.Distance(Player.main.transform.position, __instance.transform.position) > 150) return;

            // react to nearby dangerous leviathans
            if (LeviathanNames.Where(x => __instance.name.Contains(x)).Any())
            {
                foreach (var component in Player.main.GetVehicle().GetComponentsInChildren<IVehicleStatusListener>())
                {
                    component.OnNearbyLeviathan();
                }
            }
        }
    }
}
