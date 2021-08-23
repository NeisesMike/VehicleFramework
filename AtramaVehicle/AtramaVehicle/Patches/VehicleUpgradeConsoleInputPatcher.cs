using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace AtramaVehicle
{
    [HarmonyPatch(typeof(VehicleUpgradeConsoleInput))]
    public class VehicleUpgradeConsoleInputPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnClosePDA")]
        public static void OnClosePDAPrefix(VehicleUpgradeConsoleInput __instance)
        {
            if (__instance.gameObject.name == "Upgrades-Panel" && __instance.transform.parent.parent.name.Contains("Atrama"))
            {
                var vehicle = __instance.transform.parent.parent.GetComponentInChildren<AtramaVehicle>();
                vehicle.updateModules();
            }
        }

        /*
        [HarmonyPrefix]
        [HarmonyPatch("UpdateVisuals")]
        public static bool UpdateVisualsPrefix(VehicleUpgradeConsoleInput __instance)
        {
            if(__instance.gameObject.name == "Upgrades-Panel" && __instance.transform.parent.parent.name.Contains("Atrama"))
            {
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("Update")]
        public static bool UpdatePrefix(VehicleUpgradeConsoleInput __instance)
        {
            if (__instance.gameObject.name == "Upgrades-Panel" && __instance.transform.parent.parent.name.Contains("Atrama"))
            {
                return false;
            }
            return true;
        }
        */

    }
}
