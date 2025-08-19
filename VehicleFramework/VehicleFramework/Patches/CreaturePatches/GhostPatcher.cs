using HarmonyLib;
using UnityEngine;
using VehicleFramework.VehicleTypes;

// PURPOSE: configure how much damage ghost leviathans can do
// VALUE: Moderate. Convenient for developers.

namespace VehicleFramework.Patches.CreaturePatches
{
    [HarmonyPatch(typeof(GhostLeviathanMeleeAttack))]
    class GhostPatcher
    {
        /*
         * This patch changes how much damage Ghosts will do to ModVehicles.
         * Ghosts will do:
         * 85 to Seamoth/Prawn
         * 250 to Cyclops
         */
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GhostLeviathanMeleeAttack.GetBiteDamage))]
        public static void GetBiteDamagePostfix(GhostLeviathanMeleeAttack __instance, ref float __result, GameObject target)
        {
            ModVehicle mv = target.GetComponent<ModVehicle>();
            if (mv == null) return;

            TechType techType = CraftData.GetTechType(__instance.gameObject);
            if (techType == TechType.GhostLeviathan)
            {
                __result = mv.GhostAdultBiteDamage;
            }
            else if (techType == TechType.GhostLeviathanJuvenile)
            {
                __result = mv.GhostJuvenileBiteDamage;
            }
            else
            {
                Logger.Error("ERROR: Unrecognized ghost leviathan");
            }
        }
    }
}
