using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace VehicleFramework.Patches.LeviathanPatches
{
    [HarmonyPatch(typeof(GhostLeviathanMeleeAttack))]
    class GhostPatcher
    {
        /*
         * This patch changes how much damage Ghosts will do to ModVehicles.
         * Ghosts will do:
         * 85 to Seamoth/Prawn
         * 250 to Cyclops
         * TODO: we should include this in the VehicleEntry struct, to make it configurable
         */
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GhostLeviathanMeleeAttack.GetBiteDamage))]
        public static void GetBiteDamagePostfix(GhostLeviathanMeleeAttack __instance, ref float __result, GameObject target)
        {
            if(target.GetComponentInParent<ModVehicle>() != null)
            {
                TechType techType = CraftData.GetTechType(__instance.gameObject);
                string myTechType = techType.AsString(true);
                if (myTechType == "ghostleviathan")
                {
                    __result = 150f;
                }
                else if (myTechType == "ghostleviathanjuvenile")
                {
                    __result = 100f;
                }
                else
                {
                    Logger.Error("ERROR: Unrecognized ghost leviathan");
                }
            }
        }
    }
}
