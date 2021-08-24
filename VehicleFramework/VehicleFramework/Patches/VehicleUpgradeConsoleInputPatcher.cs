using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace VehicleFramework
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
                /*
                var vehicle = __instance.transform.parent.parent.GetComponentInChildren<AtramaVehicle>();
                vehicle.updateModules();
                */
            }
        }
    }
}
