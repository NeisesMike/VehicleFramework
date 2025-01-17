using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;

namespace VehicleFramework.Patches.CompatibilityPatches
{
	[HarmonyPatch(typeof(Creature))]
	[HarmonyPatch(nameof(Creature.ChooseBestAction))]
	class CreaturePatcher
	{
		[HarmonyPostfix]
		public static void Postfix(Creature __instance)
		{
            if (!(Player.main.GetVehicle() is ModVehicle mv))
            {
                return;
            }

            // react to nearby dangerous leviathans
            // I'd like for this to depend on a stealth module being installed,
            // but I'm not sure how to accomplish that,
            // so frankly whatever.
            // update: I could just check mv.GetCurrentUpgrades for the stealth upgrade's name
            // ha ha, don't want to
            if ((__instance.name.Contains("GhostLeviathan") || __instance.name.Contains("ReaperLeviathan") || __instance.name.Contains("SeaDragon")) && Vector3.Distance(Player.main.transform.position, __instance.transform.position) < 150)
            {
                foreach (var component in Player.main.GetVehicle().GetComponentsInChildren<IVehicleStatusListener>())
                {
                    component.OnNearbyLeviathan();
                }
			}
		}
	}
}
