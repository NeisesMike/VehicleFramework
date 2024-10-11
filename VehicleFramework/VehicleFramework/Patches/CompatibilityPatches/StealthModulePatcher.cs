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
            if ((__instance.name.Contains("GhostLeviathan") || __instance.name.Contains("ReaperLeviathan") || __instance.name.Contains("SeaDragon")) && Vector3.Distance(Player.main.transform.position, __instance.transform.position) < 150)
			{
				mv.NotifyStatus(VehicleStatus.OnNearbyLeviathan);
			}
		}
	}
}
